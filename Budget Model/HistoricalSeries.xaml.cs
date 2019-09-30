﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Configurations;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Budget_Model.Models;

namespace Budget_Model
{
    /// <summary>
    /// Interaction logic for HistoricalSeries.xaml
    /// </summary>
    public partial class HistoricalSeries : Page
    {
        public Func<double, string> NetWorthFormatter { get; set; }
        public Func<double, string> MonthFormatter { get; set; }
        public Func<double, string> CurrencyFormatter { get; set; }
        List<Task> tasks = new List<Task>();
        string[] categories = null;
        public string monthformat { get; set; } = "MMM yyy";
        public double step { get; set; } = 1;
        public class DateModel
        {
            public DateTime DateTime { get; set; }
            public double Value { get; set; }
        }
        public IEnumerable<Holder> HoldersItems { get; set; }

        public HistoricalSeries()
        {
            InitializeComponent();
            Tuple<DateTime, DateTime> dates = BankTransaction.GetDates("Home");
            date_month_to.SelectedDateChanged -= SelectionChanged;
            date_month_from.SelectedDateChanged -= SelectionChanged;
            if (dates != null)
            {
                date_month_to.SelectedDate = new DateTime(dates.Item2.Year, dates.Item2.Month, 1).AddMonths(1).AddDays(-1);
                date_month_to.DisplayDate = new DateTime(dates.Item2.Year, dates.Item2.Month, 1).AddMonths(1).AddDays(-1);
                date_month_from.SelectedDate = new DateTime(dates.Item2.Year, dates.Item2.Month, 1).AddMonths(-11);
                date_month_from.DisplayDate = new DateTime(dates.Item2.Year, dates.Item2.Month, 1).AddMonths(-11);
            }
            else
            {
                date_month_to.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                date_month_to.DisplayDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                date_month_from.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11).AddDays(-1);
                date_month_from.DisplayDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11).AddDays(-1);
            }
            date_month_to.SelectedDateChanged += SelectionChanged;
            date_month_from.SelectedDateChanged += SelectionChanged;

            categories = Fillcategories();
            comboCategory.ItemsSource = categories;
            comboCategory.SelectionChanged -= Category_Changed;
            comboCategory.SelectedIndex = 0;
            comboCategory.SelectionChanged += Category_Changed;

            Holder holder = new Holder();
            HoldersItems = holder.HolderCollection(true);
            comboFor.DataContext = this;
            comboFor.SelectedIndex = 0;

            var dateConfig = Mappers.Xy<DateModel>()
                        .X(dateModel => dateModel.DateTime.AddDays(-16.5).Ticks / (TimeSpan.FromDays(1).Ticks * 30.44))
                        .Y(dateModel => dateModel.Value);

            //save the mapper globally.
            Charting.For<DateModel>(dateConfig);
            CurrencyFormatter = value => value.ToString("C0");
            NetWorthFormatter = value => new DateTime((long)((value < 0 ? 0 : value) * TimeSpan.FromDays(1).Ticks * 30.44)).ToString("MMM yyyy");
            MonthFormatter = value => new DateTime((long)((value < 0 ? 0:value) * TimeSpan.FromDays(1).Ticks * 30.44)).AddDays(16.5).ToString(monthformat);

            //load all charts
            alertLoading.Visibility = System.Windows.Visibility.Visible;

            Make_Grid();
            tasks.Add(
                Task.Run(() => {
                    Savings_Chart();
                })
            );
            tasks.Add(
                Task.Run(() => {
                    Category_Chart();
                })
            );

            tasks.Add(
                Task.Run(() => {
                    Stacked_Chart();
                })
            );

            tasks.Add(
                Task.Run(() => {
                    NetWorth_Chart();
                })
            );

            Task.Run(() =>
            {
                Task.WaitAll(tasks.ToArray());
                Dispatcher.Invoke(() =>
                {
                    alertLoading.Visibility = System.Windows.Visibility.Hidden;
                });
            });
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (date_month_from.IsLoaded && date_month_to.IsLoaded //&& alertLoading.Visibility != System.Windows.Visibility.Visible
                && !(((DateTime)e.RemovedItems[0]).Year == ((DateTime)e.AddedItems[0]).Year && ((DateTime)e.RemovedItems[0]).Month == ((DateTime)e.AddedItems[0]).Month))
            {
                DateTime from_month = new DateTime(Convert.ToDateTime(date_month_from.SelectedDate).Year, Convert.ToDateTime(date_month_from.SelectedDate).Month, 1);
                DateTime to_month = Convert.ToDateTime(date_month_to.SelectedDate);
                if ((to_month - from_month).TotalDays < 93)
                {
                    monthformat = "M/d/yyyy";
                    NetSavingsChartTitle.Text = NetSavingsChartTitle.Text.Replace("Monthly", "Cumulative Weekly");
                    CategoryChartTitle.Text = CategoryChartTitle.Text.Replace("Month", "Week");
                    StackedChartTitle.Text = StackedChartTitle.Text.Replace("Monthly", "Weekly");
                    NetWorthChartTitle.Text = NetWorthChartTitle.Text.Replace("Monthly", "Daily");
                }
                else
                {
                    monthformat = "MMM yyyy";
                    NetSavingsChartTitle.Text = NetSavingsChartTitle.Text.Replace("Cumulative Weekly", "Monthly");
                    CategoryChartTitle.Text = CategoryChartTitle.Text.Replace("Week", "Month");
                    StackedChartTitle.Text = StackedChartTitle.Text.Replace("Weekly", "Monthly");
                    NetWorthChartTitle.Text = NetWorthChartTitle.Text.Replace("Daily", "Monthly");
                }

                alertLoading.Visibility = System.Windows.Visibility.Visible;

                Make_Grid(true);
                tasks.Add(
                    Task.Run(() => {
                        Savings_Chart(true);
                    })
                );

                tasks.Add(
                    Task.Run(() => {
                        Category_Chart(true);
                    })
                );

                tasks.Add(
                    Task.Run(() => {
                        Stacked_Chart(true);
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        NetWorth_Chart(true);
                    })
                );


                Task.Run(() =>
                {
                    Task.WaitAll(tasks.ToArray());
                    Dispatcher.Invoke(() =>
                    {
                        alertLoading.Visibility = System.Windows.Visibility.Hidden;
                    });
                });
            }
        }
        private void Category_Changed(object sender, SelectionChangedEventArgs e)
        {
            alertLoading.Visibility = System.Windows.Visibility.Visible;
            tasks.Add(
                    Task.Run(() => {
                        Category_Chart(true);
                    }).ContinueWith(delegate {
                        Dispatcher.Invoke(() =>
                        {
                            alertLoading.Visibility = System.Windows.Visibility.Hidden;
                        });
                    })
                );
            Task.Run(() =>
            {
                Task.WaitAll(tasks.ToArray());
                Dispatcher.Invoke(() =>
                {
                    alertLoading.Visibility = System.Windows.Visibility.Hidden;
                });
            });
        }
        private void GridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (date_month_from.IsLoaded && date_month_to.IsLoaded)
            {
                alertLoading.Visibility = System.Windows.Visibility.Visible;
                Make_Grid(true);
                Task.Run(() =>
                {
                    Task.WaitAll(tasks.ToArray());
                    Dispatcher.Invoke(() =>
                    {
                        alertLoading.Visibility = System.Windows.Visibility.Hidden;
                    });
                });
            }
        }

        public SeriesCollection Series_NetWorth { get; set; } = new SeriesCollection();

        public void NetWorth_Chart(bool change = false)
        {
            DataTable dt = new DataTable();
            DateTime from_month = new DateTime();
            DateTime to_month = new DateTime();
            string datatable = "MonthlyBalance";
            Dispatcher.Invoke(() =>
            {
                from_month = new DateTime(Convert.ToDateTime(date_month_from.SelectedDate).Year, Convert.ToDateTime(date_month_from.SelectedDate).Month, 1);
                to_month = Convert.ToDateTime(date_month_to.SelectedDate);
            });
            if ((to_month - from_month).TotalDays < 93)
                datatable = "DailyBalance";

            dt = MonthlyStatement.GetHistoricalNetWorth(datatable, from_month, to_month);

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                Series_NetWorth.Clear();
                foreach (Holder holder in HoldersItems)
                {
                    if (dt.AsEnumerable().Any(r => r["holder"].ToString() == holder.HolderName))
                    {
                        Series_NetWorth.Add(new LineSeries()
                        {
                            Values = new ChartValues<DateModel>(dt.AsEnumerable().Where(r => r["holder"].ToString() == holder.HolderName)
                        .Select(r => new DateModel { DateTime = Convert.ToDateTime(r["period"]), Value = r.Field<double?>("NetWorth") ?? 0 })),
                            Title = holder.HolderName
                        });
                    }
                }

                if (!change)
                {
                    NetWorthChart.DataContext = this;
                }
            }));
        }
        
        public SeriesCollection Series_Savings { get; set; } = new SeriesCollection();
        public void Savings_Chart(bool change = false)
        {
            DataTable dt = new DataTable();
            DateTime from_month = new DateTime();
            DateTime to_month = new DateTime();
            Dispatcher.Invoke(() =>
            {
                from_month = new DateTime(Convert.ToDateTime(date_month_from.SelectedDate).Year, Convert.ToDateTime(date_month_from.SelectedDate).Month, 1);
                to_month = Convert.ToDateTime(date_month_to.SelectedDate);
            });
            dt = MonthlyStatement.GetHistoricalSavings(from_month, to_month);

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                Series_Savings.Clear();
                foreach (Holder holder in HoldersItems)
                {
                    if (dt.AsEnumerable().Any(r => r["holder"].ToString() == holder.HolderName))
                    {
                        Series_Savings.Add(new LineSeries()
                        {
                            Values = new ChartValues<DateModel>(dt.AsEnumerable().Where(r => r["holder"].ToString() == holder.HolderName)
                        .Select(r => new DateModel { DateTime = Convert.ToDateTime(r["period"]), Value = r.Field<double?>("amount") ?? 0 })),
                            Title = holder.HolderName
                        });
                    }
                }
                if (!change)
                {
                    SavingsChart.DataContext = this;
                }
            }));
        }

        public SeriesCollection Series_Category { get; set; } = new SeriesCollection();
        public void Category_Chart(bool change = false)
        {
            DataTable dt = new DataTable();
            DateTime from_month = new DateTime();
            DateTime to_month = new DateTime();
            string selected_category = "";

            Dispatcher.Invoke(() =>
            {
                from_month = new DateTime(Convert.ToDateTime(date_month_from.SelectedDate).Year, Convert.ToDateTime(date_month_from.SelectedDate).Month, 1);
                to_month = Convert.ToDateTime(date_month_to.SelectedDate);
                selected_category = comboCategory.SelectedItem.ToString();
            });
            string freq = "Monthly";
            if ((to_month - from_month).TotalDays < 93)
                freq = "Weekly";
            
            dt = MonthlyStatement.GetHistoricalTransactionsByCategory(selected_category, freq, from_month, to_month);
            
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                Series_Category.Clear();
                foreach (Holder holder in HoldersItems)
                {
                    if (dt.AsEnumerable().Any(r => r["holder"].ToString() == holder.HolderName))
                    {
                        Series_Category.Add(new LineSeries()
                        {
                            Values = new ChartValues<DateModel>(dt.AsEnumerable().Where(r => r["holder"].ToString() == holder.HolderName)
                        .Select(r => new DateModel { DateTime = Convert.ToDateTime(r["period"]), Value = r.Field<double?>("amount") ?? 0 })),
                            Title = holder.HolderName,
                            Fill = Brushes.Transparent
                        });
                    }
                }
                if (!change)
                {
                    CategoryChart.DataContext = this;
                }
            }));
        }

        public SeriesCollection StackedSeriesCollection { get; set; } = new SeriesCollection();
        public void Stacked_Chart(bool change = false)
        {
            DataTable dt = new DataTable();
            DateTime from_month = new DateTime();
            DateTime to_month = new DateTime();

            Dispatcher.Invoke(() =>
            {
                from_month = new DateTime(Convert.ToDateTime(date_month_from.SelectedDate).Year, Convert.ToDateTime(date_month_from.SelectedDate).Month, 1);
                to_month = Convert.ToDateTime(date_month_to.SelectedDate);

                StackedSeriesCollection.Clear();
            });
            string freq = "Monthly";
            if ((to_month - from_month).TotalDays < 93)
                freq = "Weekly";
            
            dt = MonthlyStatement.GetHistoricalTransactionsByCategory("", freq, from_month, to_month);

            string[] selectcategories = categories.Skip(3).ToArray();

            Dispatcher.Invoke(() =>
            {
                foreach (string category in selectcategories)
                {
                    ChartValues<DateModel> chartvalues = new ChartValues<DateModel>(dt.AsEnumerable().Where(r => r["category"].ToString() == category)
                            .Select(r => new DateModel { DateTime = Convert.ToDateTime(r["period"]), Value = r.Field<double?>("amount") ?? 0 }));
                    if (chartvalues.Count != 0)
                        StackedSeriesCollection.Add(new StackedColumnSeries
                        {
                            Values = chartvalues,
                            StackMode = StackMode.Values,
                            Title = category
                        });
                }
            });

            if (!change)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, (Action)(() =>
                {
                    DataContext = this;
                }));
            }
        }
        
        public void Make_Grid(bool change = false)
        {
            DataTable dt = new DataTable();
            string[] selectcategories = categories.Skip(3).ToArray();
            string selected_person = HoldersItems.Where(r => r.IsChecked == true).Select(x => x.HolderName).First();

            dt.Columns.Add("RowHeader");
            foreach (string rowheader in new string[] { "Mean", "Median", "Standard Deviation"})
            {
                DataRow row = dt.NewRow();
                row["RowHeader"] = rowheader;
                dt.Rows.Add(row);
            }

            Dispatcher.Invoke(() => {
                DataGridAverage.DataContext = dt;
            });

            DateTime from_month = from_month = new DateTime(Convert.ToDateTime(date_month_from.SelectedDate).Year, Convert.ToDateTime(date_month_from.SelectedDate).Month, 1);
            DateTime to_month = Convert.ToDateTime(date_month_to.SelectedDate);

            //savings
            Task saving_task = Task.Run(() =>
            {
                Tuple<string, string, string> mean_median_std = MonthlyStatement.GetHistoricalAggregateCategoryAmount("savings", selected_person, from_month, to_month);
                Dispatcher.Invoke(() => {
                    dt.Columns.Add("savings");

                    dt.Rows[0]["savings"] = mean_median_std.Item1;
                    dt.Rows[1]["savings"] = mean_median_std.Item2;
                    dt.Rows[2]["savings"] = mean_median_std.Item3;

                    DataGridAverage.DataContext = dt;
                });
            });

            //expenses
            Task expense_task = Task.Run(() =>
            {
                Tuple<string, string, string> mean_median_std = MonthlyStatement.GetHistoricalAggregateCategoryAmount("expenses", selected_person, from_month, to_month);
                Dispatcher.Invoke(() => {
                    dt.Columns.Add("expenses");

                    dt.Rows[0]["expenses"] = mean_median_std.Item1;
                    dt.Rows[1]["expenses"] = mean_median_std.Item2;
                    dt.Rows[2]["expenses"] = mean_median_std.Item3;

                    DataGridAverage.DataContext = dt;
                });
            });

            //categories
            Task categories_task = Task.Run(() =>
            {
                foreach (string category in BudgetCategory.GetCategories())
                {
                    if (category != "Miscellaneous" && !category.Contains("Investment"))
                    {
                        Tuple<string, string, string> mean_median_std = MonthlyStatement.GetHistoricalAggregateCategoryAmount(category, selected_person, from_month, to_month);
                        Dispatcher.Invoke(() => {
                            if (!change)
                            {
                                DataGridTextColumn c = new DataGridTextColumn();
                                c.Header = category;
                                c.IsReadOnly = true;
                                c.CellStyle = (System.Windows.Style)FindResource("RightAlignment");
                                c.Binding = new System.Windows.Data.Binding(category);
                                DataGridAverage.Columns.Add(c);
                            }

                            dt.Columns.Add(category);
                            dt.Rows[0][category] = mean_median_std.Item1;
                            dt.Rows[1][category] = mean_median_std.Item2;
                            dt.Rows[2][category] = mean_median_std.Item3;
                        });
                    }
                }
                
                /**
                SqlDataAdapter adapter = new SqlDataAdapter();
                string qry = @"SELECT a.category, AVG(a.amount) as mean, median,
                        STDEV(a.amount) as std FROM " + (selected_person == "Home" ? "Home" : "Individual") + @"_Monthly a 
                        INNER JOIN categories on a.category = categories.category
                        INNER JOIN (select distinct category, PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY amount) OVER (PARTITION BY category) as median 
                         from " + (selected_person == "Home" ? "Home" : "Individual") + @"_Monthly a 
                          WHERE a.period BETWEEN '" + from_month + @"' and '" + to_month + "' " + (selected_person == "Home" ? "" : "AND a.holder = '" + selected_person + "' ") + @"
                         AND a.category != 'Miscellaneous' AND a.category NOT LIKE '%Retirement%' AND a.category NOT LIKE '%Investment%'
                         )b ON a.category = b.category
                          WHERE a.period BETWEEN '" + from_month + @"' and '" + to_month + @"' " + (selected_person == "Home" ? "" : "AND a.holder = '" + selected_person + "' ") + @"
                         AND a.category != 'Miscellaneous' AND a.category NOT LIKE '%Retirement%' AND a.category NOT LIKE '%Investment%'
                         GROUP BY a.category, category_order, median ORDER BY category_order";
                
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["BudgetDataConnectionString"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(qry, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Call Read before accessing data.
                    while (reader.Read())
                    {
                        Dispatcher.Invoke(() => {
                            string category = reader.GetString(reader.GetOrdinal("category"));
                            if (!change)
                            {
                                DataGridTextColumn c = new DataGridTextColumn();
                                c.Header = category;
                                c.IsReadOnly = true;
                                c.CellStyle = (System.Windows.Style)FindResource("RightAlignment");
                                c.Binding = new System.Windows.Data.Binding(category);
                                DataGridAverage.Columns.Add(c);
                            }

                            dt.Columns.Add(category);
                            string mean = (!reader.IsDBNull(reader.GetOrdinal("mean")) ? reader.GetDouble(reader.GetOrdinal("mean")).ToString("C2") : "");
                            dt.Rows[0][category] = (!reader.IsDBNull(reader.GetOrdinal("mean")) ? reader.GetDouble(reader.GetOrdinal("mean")).ToString("C2") : "");
                            dt.Rows[1][category] = (!reader.IsDBNull(reader.GetOrdinal("median")) ? reader.GetDouble(reader.GetOrdinal("median")).ToString("C2") : "");
                            dt.Rows[2][category] = (!reader.IsDBNull(reader.GetOrdinal("std")) ? reader.GetDouble(reader.GetOrdinal("std")).ToString("C2") : "");
                        });
                    }
                    // Call Close when done reading.
                    reader.Close();
                    conn.Close();
                }
                 **/
                Dispatcher.Invoke(() => {
                    DataGridAverage.DataContext = dt;
                });
            });

            tasks.Add(categories_task);
            tasks.Add(saving_task);
            tasks.Add(expense_task);
        }

        public string[] Fillcategories()
        {
            List<string> categories = new List<string>();
            categories.Add("All Expenses ex. Miscellaneous");
            categories.Add("All Income ex. Miscellaneous");
            categories.Add("Gross Salary");
            categories.AddRange(BudgetCategory.GetCategories(true));
            return categories.ToArray();
        }
    }
}