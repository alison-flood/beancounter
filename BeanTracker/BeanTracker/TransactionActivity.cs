using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using Mono.Data.Sqlite;
using System.Collections;

namespace BeanTracker
{
	[Activity (Label = "TransactionActivity")]			
	public class TransactionActivity : ListActivity
	{
		private string dbPath = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "beanbase.db3");
		private TransactionAdapter transAdapter = null;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			GrabTransactions ();
		}

		private void GrabTransactions()
		{
			transAdapter = new TransactionAdapter (this);

			Mono.Data.Sqlite.SqliteConnection connection = null;

			connection = new SqliteConnection ("Data Source=" + dbPath);
			connection.Open ();

			// query the database to prove data was inserted!
			using (var contents = connection.CreateCommand ()) {
				contents.CommandText = "SELECT [Id], [TransactionDate], [Amount], [Description] from [BeanTransactions] ORDER BY [TransactionDate] DESC";
				var r = contents.ExecuteReader ();
				while (r.Read ()){
					int id = (int)(long)r ["Id"];
					DateTime date = (DateTime)r ["TransactionDate"];
					decimal amount = (decimal)r ["Amount"];
					string description = (string)r ["Description"];
					transAdapter.Add(id, date, amount, description);
				}
			}
			connection.Close ();

			ListAdapter = transAdapter;
		}

		protected override void OnListItemClick(ListView l, View v, int position, long id){
			var t = transAdapter.transactions[position];
//			Android.Widget.Toast.MakeText(this, t, Android.Widget.ToastLength.Short).Show();
			Android.App.AlertDialog.Builder builder = new AlertDialog.Builder (this);
			AlertDialog alertDialog = builder.Create ();
			alertDialog.SetTitle ("Delete Transaction?");
			alertDialog.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			alertDialog.SetMessage ("Do you wish to delete the selected transaction?");
			alertDialog.SetButton ("Yes", (s, EventArgs) => {
				DeleteTransaction(transAdapter.transactions[position].id);
				transAdapter.transactions.RemoveAt(position);
			});
			alertDialog.Show ();
		}

		private void DeleteTransaction(int id){
			Mono.Data.Sqlite.SqliteConnection connection = null;

			connection = new SqliteConnection ("Data Source=" + dbPath);
			connection.Open ();

			// query the database to prove data was inserted!
			using (var contents = connection.CreateCommand ()) {
				contents.CommandText = "DELETE FROM [BeanTransactions] WHERE Id = " + id;
				contents.CommandType = System.Data.CommandType.Text;
				contents.ExecuteNonQuery ();
			}
			connection.Close ();
		}
	}
}

