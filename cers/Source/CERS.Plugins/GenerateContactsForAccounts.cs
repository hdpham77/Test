using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.Plugins
{
	[Plugin("Generate Contact Records for Accounts (with missing Contacts)", Description = "Generates Contact records for Accounts that do not have existing Contact records.", DeveloperName = "Mike")]
	public class GenerateContactsForAccounts : SimplePlugin
	{
		protected override void DoWork()
		{
			var accountsWithNoContacts = from ac in DataModel.AccountsWithNoContacts
										 join a in DataModel.vAccounts on ac.ID equals a.ID
										 select a;
			int total = accountsWithNoContacts.Count();
			OnNotification("Found " + total + " Account(s) with No Contact Record");

			int index = 0;

			var data = accountsWithNoContacts.ToList();

			foreach (var account in data)
			{
				Repository.Contacts.EnsureExists(account);
				OnNotification("Processed " + account.ID + " - " + account.DisplayName + " (" + account.UserName + ")");
				CalculateProgress(index, total);
				index++;
			}
		}
	}
}