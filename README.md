#Budget Model App

A simple Windows desktop app in WPF that tracks monthly expense/income budget, net worth, and financial investments across all bank and brokerage accounts.  

- More powerful and scalable than building and maintaining a spreadsheet
- Can program the app to read any account report/statement format, and show any kind of analysis desired without restriction
- Unlike personal finance app solutions like Mint, Quicken and Quickbooks, there is no automatic linking/synchronizing with accounts, so bank passwords and login details are never entrusted to companies or saved in any database

###Quick Start: Preview the app functions with sample data
Run the application Budget Model.exe in the folder Budget Model\bin\Debug\. The app will first show the **Budget Statement** screen, with sample data that has already been categorized. Use the buttons at the top right to navigate to other screens, including the **Historical Budget and Net Worth Analysis** screen and the **Investments** screen.
![Budget Statement screen](/images/Main window.png)
![Budget Statement screen](/images/Historical Analysis - Net Worth, monthly savings, income, expenses.png)
![Budget Statement screen](/images/Investments analysis.png)

The sample data which was uploaded to the app is found in the folder \test_data.  The **Data & Definitions** screen is where bank activity reports and brokerage statements can get uploaded, and where categories can be set for purchases or income that appears on statements.  The categorization consists of keyword matching, that will assign the same category to all statement items that have the same set of words in their descriptions.
![Budget Statement screen](Categorizing transaction items in accounts.png)

###Quick Start: Personalize 
A number of items need to be set up for first use.
1. FinancialInstitutions_Sample.cs in the Models folder
	The *GetFinancialInstitutions* method in the *FinancialInstitution* class returns the list of bank accounts and brokerage accounts at all financial institutions which this app analyzes. Before first use, this list must be written in the code.  If the method is not implemented, the sample list found in the derived abstract class *BaseFinancialInstitution* in the file  *IFinancialInstitution.cs* will be used, which includes *BankSample* and *BrokerageSample*. 
2. Holders_Sample.cs in the Models folder
	The *HolderCollection* method in the *Holder* class should be written to list the account holders.  Households may have more than one account holder, and the app allows the ability to view analysis for each account holder, as well as at the aggregated household level, named *"Home"*.  If the method is not implemented, the sample list found in the derived abstract class *BaseHolder* in the file  *IHolder.cs* will be used, which includes *Person1* and *Person2*. 
3. ExcelImport_Sample.cs in the Helpers folder
	The ExcelImport class implements the method that reads CSV files of bank activity reports and statements.  To retrieve daily transactions and asset values from the CSV files, the class needs to be written to read the data points from the appropriate columns for the different formats that different reports/statements may have.  It uses the CsvHelper library

	Particularities of the data
	- Gross salary
		Gross salary does not usually appear on bank statements -- only net pay usually shows as an inflow in bank activity. However, gross salary can be specified manually in the **Data & Definitions** window.  Once set, the amount set for gross salary will automatically carry forward to the next month, but can be manually changed (e.g. if there is a salary raise)
	- Initial balance
		The app computes the balance on your bank accounts based on a rolling sum of all your transactions.  Therefore, to show accurate bank account balances, the app needs the full history of bank transactions since the account was opened.  If a limited history of transactions for one account is uploaded, the initial account balance immediately prior to the earliest date of uploaded transactions must also be provided as a separate initial "transaction".

###Other files to be modified
The following files can be personalized.

1. Configuration files
	- Connections.config
		This file contains the connection string to the database where all the data will be saved.  As a sample, it currently points to the SQLite database *BudgetData_Sample.db* in the *App_Data* folder.
	- CustomAppSettings.config
		This file contains API keys or passwords that are used to integrate stock pricing and bond yield APIs to track investment performance of trading accounts.  Here, the free [Alpha Vantage API](https://www.alphavantage.co/) is used to display stock/ETF prices in the **Investments** screen.  An API key is provided when registering on the website, which must be specified in *CustomAppSettings.config* as `<appSettings><add key="alphav_key" value="[API key]"/></appSettings>`.
	When appending configuration files, please make sure to set the file properties such that the custom config files will be copied to the output directory on build. 
2. The database - SQLite
	For illustration, the app currently uses a sample database *BudgetData_Sample.db*.  In the database, the Categories and InvestmentCategories tables store the list of categories used by the application.  All other tables are filled by user input and may be modified. SQLite was used. It is lightweight and suitable for local data storage.
	
###License
This project is licensed under GNU AGPLv3, which makes the complete source code available, but any work using the code must be similarly licensed open-source under AGPL.

###Always room for improvement
- [ ] More flexibility in adding and sorting categories
- [ ] Interface to set bank accounts and holders (currently required to be set up on code-behind)
- [ ] Interface to define different report formats (currently required to be set up on code-behind)
- [ ] Retirement account integration

###Contact
[My Github Pages website](https://sharonchoong.github.io/)