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

namespace BeanTracker
{
	public class TransactionAdapter : BaseAdapter
	{
		public List<Transaction> transactions = new List<Transaction>();
		private Activity context;

		public TransactionAdapter(Activity context){
			this.context = context;
		}

		public void Add(int id, DateTime date, decimal amount, string description){
			transactions.Add(new Transaction(id, date, amount, description));
		}

		public override long GetItemId(int position)
		{
			return (long)transactions[position].id;
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return (Java.Lang.Object)(object)transactions [position];
		}

		public override int Count {
			get { return transactions.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null)
				view = context.LayoutInflater.Inflate (Android.Resource.Layout.SimpleListItem2, null);

			view.FindViewById<TextView> (Android.Resource.Id.Text1).Text = "Date: " + transactions [position].transactionDate.ToString ("dd/MM/yyyy") + " - Amt: " + transactions [position].amount;
			view.FindViewById<TextView> (Android.Resource.Id.Text2).Text = transactions[position].description;
			return view;
		}

		public class Transaction
		{
			public int id;
			public DateTime transactionDate;
			public decimal amount;
			public string description;

			public Transaction(int id, DateTime transactionDate, decimal amount, string description)
			{
				this.id = id;
				this.transactionDate = transactionDate;
				this.amount = amount;
				this.description = description;
			}
		}
	}
}

