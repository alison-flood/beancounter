using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

namespace BeanTracker
{
	[Activity (Label = "BeanCounter", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private string dbPath = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "beanbase.db3");
		private decimal totalIn = 0;
		private decimal totalOut = 0;
		private string createTable = "CREATE TABLE [BeanTransactions] (Id INTEGER PRIMARY KEY AUTOINCREMENT, TransactionDate DATETIME, Amount DECIMAL(16,2), Description VARCHAR(200));";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			GrabTransactionValues ();

			Button AddExpense = FindViewById<Button> (Resource.Id.addOutButton);
			AddExpense.Click += (object sender, EventArgs e) => {
				var valueObj = FindViewById<TextView>(Resource.Id.addOut);
				decimal value = decimal.Parse(valueObj.Text);
				var descriptionObj = FindViewById<TextView>(Resource.Id.description);
				string description = descriptionObj.Text;
				AddTransaction(value, description, true);
				valueObj.Text = string.Empty;
				descriptionObj.Text = string.Empty;
			};

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.viewTransactions);
			
			button.Click += delegate {
				var intent = new Intent(this, typeof(TransactionActivity));
				StartActivity(intent);
			};

			Button addSaving = FindViewById<Button> (Resource.Id.addSaving);
			addSaving.Click += (object sender, EventArgs e) => {
				var valueObj = FindViewById<TextView>(Resource.Id.addOut);
				decimal value = decimal.Parse(valueObj.Text);
				var descriptionObj = FindViewById<TextView>(Resource.Id.description);
				string description = descriptionObj.Text;
				AddTransaction(value, description, false);
				valueObj.Text = string.Empty;
				descriptionObj.Text = string.Empty;
			};
		}

		private void AddTransaction(decimal amount, string description, bool isExpense){
			Mono.Data.Sqlite.SqliteConnection connection = new SqliteConnection ("Data Source=" + dbPath);
			connection.Open ();
			if (isExpense)
				amount *= -1;

			var commands = new [] { "INSERT INTO [BeanTransactions] (TransactionDate, Amount, Description) VALUES ('" + 
				DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "', " + amount.ToString("N2") + ", '" + description + "')"
			};

			foreach (var command in commands) {
				using (var c = connection.CreateCommand ()) {
					c.CommandText = command;
					c.CommandType = System.Data.CommandType.Text;
					c.ExecuteNonQuery();
				}
			}

			if (isExpense)
				totalOut += amount;
			else
				totalIn += amount;
			SetLabels ();
			connection.Close ();
		}

		private void SetLabels(){
			var savings = FindViewById<TextView> (Resource.Id.savings);
			var spendings = FindViewById<TextView> (Resource.Id.spendings);
			var remaining = FindViewById<TextView> (Resource.Id.remaining);

			savings.Text = totalIn.ToString ("C");
			spendings.Text = totalOut.ToString ("C");
			remaining.Text = (totalIn + totalOut).ToString ("C");
		}

		private void GrabTransactionValues()
		{
			bool exists = File.Exists (dbPath);
			Mono.Data.Sqlite.SqliteConnection connection = null;

			if (!exists) {
				Mono.Data.Sqlite.SqliteConnection.CreateFile (dbPath);

				connection = new SqliteConnection ("Data Source=" + dbPath);

				connection.Open ();
//				var commands = new[] {
//					"CREATE TABLE [BeanTransactions] (Id INTEGER PRIMARY KEY AUTOINCREMENT, TransactionDate DATETIME, Amount DECIMAL(16,2), Description VARCHAR(200))"
//				};

				string[] commands = new string[] {
					createTable
				};

				foreach (var command in commands) {
					using (var c = connection.CreateCommand ()) {
						c.CommandText = command;
						c.CommandType = System.Data.CommandType.Text;
						c.ExecuteNonQuery();
					}
				}
				connection.Close ();
			}

			connection = new SqliteConnection ("Data Source=" + dbPath);
			connection.Open ();

			// query the database to prove data was inserted!
			using (var contents = connection.CreateCommand ()) {
				contents.CommandText = "SELECT SUM(Amount) amt from [BeanTransactions] WHERE Amount < 0";
				contents.CommandType = System.Data.CommandType.Text;
				var r = contents.ExecuteReader ();
				while (r.Read ()){
					if (r [0] == DBNull.Value)
						totalOut = 0;
					else
						totalOut = decimal.Parse(r [0].ToString());
				}
			}

			// query the database to prove data was inserted!
			using (var contents = connection.CreateCommand ()) {
				contents.CommandText = "SELECT SUM(Amount) amt from [BeanTransactions] WHERE Amount > 0";
				var r = contents.ExecuteReader ();
				while (r.Read ()){
					if (r [0] == DBNull.Value)
						totalIn = 0;
					else
						totalIn = decimal.Parse(r [0].ToString());
				}
			}

			SetLabels ();
			try{
				connection.Close ();
			} catch(Exception e){
				int pause = 0;
			}
		}
	}
}


