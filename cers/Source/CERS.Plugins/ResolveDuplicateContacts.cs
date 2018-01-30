using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CERS.Model;
using UPF;

namespace CERS.Plugins
{
    [Plugin("Resolve Duplicate Contacts", Description = "Finds Duplicate Contacts by Email and Migrates Regulator/Organization Relationships to create a single Contact for that Email.", DeveloperName = "Mike", EnableLog = false, Order = 3)]
    public class ResolveDuplicateContacts : SimplePlugin
    {
        public bool Commit { get; set; }

        protected override void DoWork()
        {
            Commit = true;

            OnNotification("Identifying Email Addresses that are associated with more than one Contact...");
            var duplicateEmails = (from c in DataModel.Contacts
                                   where !c.Voided
                                   group c by c.Email into g
                                   where g.Count() > 1
                                   select new DuplicateEmail
                                   {
                                       Email = g.Key,
                                       Count = g.Count()
                                   }).ToList();

            int duplicateEmailCount = duplicateEmails.Count();
            OnNotification("Found Duplicate Emails: " + duplicateEmailCount);
            foreach (var duplicateEmail in duplicateEmails)
            {
                Process(duplicateEmail);
            }
        }

        private void Process(DuplicateEmail duplicateEmail)
        {
            OnNotification("Processing Email: " + duplicateEmail.Email + " (" + duplicateEmail.Count + " Contact(s))");

            string targetEmail = duplicateEmail.Email;

            //Let's see if we can find a contact for this email address that has an account.
            var contactsWithAccount = DataModel.Contacts.Where(p => p.Email == targetEmail && !p.Voided && p.AccountID != null).ToList();

            //Let's find all the Contacts that don't have an account.
            var contactsWithoutAccount = DataModel.Contacts.Where(p => p.Email == targetEmail && !p.Voided && p.AccountID == null).ToList();

            if (contactsWithAccount.Count == 0 && contactsWithoutAccount.Count > 0)
            {
                Reconcile(targetEmail, contactsWithoutAccount);
            }
            else
            {
                Reconcile(targetEmail, contactsWithAccount, contactsWithoutAccount);
            }
        }

        public void Reconcile(string targetEmail, List<Contact> contactsWithAccount, List<Contact> contactsWithoutAccount)
        {
            if (contactsWithAccount.Count == 1 && contactsWithoutAccount.Count > 0)
            {
                var contactWithAccount = contactsWithAccount.FirstOrDefault();
                ReconcileMultipleContactsNoAccountIntoSingleContactWithAccount(contactWithAccount, contactsWithoutAccount);
            }
            else if (contactsWithAccount.Count > 1 && contactsWithoutAccount.Count == 0)
            {
                ReconcileMultipleContactsWithAccountIntoSingleAccount(targetEmail, contactsWithAccount);
            }
            else if (contactsWithAccount.Count > 1 && contactsWithoutAccount.Count > 0)
            {
                ReconcileMultipleContactsWithAccountAndMultipleContactsWithoutAccount(targetEmail, contactsWithAccount, contactsWithoutAccount);
            }
            else
            {
                OnNotification("--> WACKY SITUATION HERE...MANUAL MERGE NECESSARY!");
            }
        }

        private void ReconcileMultipleContactsWithAccountAndMultipleContactsWithoutAccount(string targetEmail, List<Contact> contactsWithAccount, List<Contact> contactsWithoutAccount)
        {
            OnNotification("--> Reconcile Multiple Contacts (No Account) with Multiple Contacts (With Account): " + targetEmail);

            var targetContact = FindMostSuitableContactWithAccountByEmail(targetEmail, contactsWithAccount);
            if (targetContact != null)
            {
                OnNotification("----> Suitable target Contact identified: " + targetContact.ID);
            }
            else
            {
                OnNotification("----> Suitable target Contact could NOT be identified. WARNING! MANUAL MERGE!");
            }

            var targetContactOrganizationMappings = GetOrganizationContacts(targetContact);
            var targetContactRegulatorMappings = GetRegulatorContacts(targetContact);

            var contactsToMerge = contactsWithoutAccount.Union(contactsWithAccount).Where(p => p.ID != targetContact.ID).ToList();

            foreach (var contact in contactsToMerge)
            {
                UpdateOrganizationContactMappings(targetContact, contact, targetContactOrganizationMappings);
                UpdateRegulatorContactMappings(targetContact, contact, targetContactRegulatorMappings);

                DeleteEvents(contact);

                //now that the entity mappings where dealt with, lets void this contact.
                OnNotification("----> Voiding Contact for ContactID: " + contact.ID);
                contact.Voided = true;
            }

            if (Commit)
            {
                DataModel.SaveChanges();
            }
        }

        private void ReconcileMultipleContactsWithAccountIntoSingleAccount(string targetEmail, List<Contact> contactsWithAccount)
        {
            OnNotification("--> Reconcile Multiple Contacts (With Account) into Single Contact: " + targetEmail);
            var targetContact = FindMostSuitableContactWithAccountByEmail(targetEmail, contactsWithAccount);
            if (targetContact != null)
            {
                OnNotification("----> Suitable target Contact identified: " + targetContact.ID);
            }
            else
            {
                OnNotification("----> Suitable target Contact could NOT be identified. WARNING! MANUAL MERGE!");
            }

            var targetContactOrganizationMappings = GetOrganizationContacts(targetContact);
            var targetContactRegulatorMappings = GetRegulatorContacts(targetContact);

            var contactsToMerge = contactsWithAccount.Where(p => p.ID != targetContact.ID).ToList();

            foreach (var contact in contactsToMerge)
            {
                UpdateOrganizationContactMappings(targetContact, contact, targetContactOrganizationMappings);
                UpdateRegulatorContactMappings(targetContact, contact, targetContactRegulatorMappings);

                DeleteEvents(contact);

                //now that the entity mappings where dealt with, lets void this contact.
                OnNotification("----> Voiding Contact for ContactID: " + contact.ID);
                contact.Voided = true;
            }

            if (Commit)
            {
                DataModel.SaveChanges();
            }
        }

        private void ReconcileMultipleContactsNoAccountIntoSingleContactWithAccount(Contact targetContact, List<Contact> contactsWithoutAccount)
        {
            OnNotification("--> Reconcile Multiple Contacts (No Account) into Single Contact (With Account): " + targetContact.Email);

            var targetContactOrganizationMappings = GetOrganizationContacts(targetContact);
            var targetContactRegulatorMappings = GetRegulatorContacts(targetContact);

            foreach (var contact in contactsWithoutAccount)
            {
                UpdateOrganizationContactMappings(targetContact, contact, targetContactOrganizationMappings);
                UpdateRegulatorContactMappings(targetContact, contact, targetContactRegulatorMappings);

                DeleteEvents(contact);

                //now that the entity mappings where dealt with, lets void this contact.
                OnNotification("----> Voiding Contact for ContactID: " + contact.ID);
                contact.Voided = true;
            }

            if (Commit)
            {
                DataModel.SaveChanges();
            }
        }

        public void Reconcile(string targetEmail, List<Contact> contactsWithoutAccount)
        {
            OnNotification("--> Reconcile Contacts (No Account) into single Contact: " + targetEmail);
            var targetContact = contactsWithoutAccount.OrderBy(p => p.CreatedOn).FirstOrDefault();
            var targetContactOrganizationMappings = GetOrganizationContacts(targetContact);
            var targetContactRegulatorMappings = GetRegulatorContacts(targetContact);

            foreach (var contact in contactsWithoutAccount.Where(p => p.ID != targetContact.ID).ToList())
            {
                UpdateOrganizationContactMappings(targetContact, contact, targetContactOrganizationMappings);
                UpdateRegulatorContactMappings(targetContact, contact, targetContactRegulatorMappings);

                DeleteEvents(contact);

                //now that the entity mappings where dealt with, lets void this contact.
                OnNotification("----> Voiding Contact for ContactID: " + contact.ID);
                contact.Voided = true;
            }

            if (Commit)
            {
                DataModel.SaveChanges();
            }
        }

        #region Helper Methods

        #region FindMostSuitableContactWithAccountByEmail Method

        public Contact FindMostSuitableContactWithAccountByEmail(string targetEmail, List<Contact> contacts)
        {
            //Let's find the best Contact with the most recent account activity and use that as our target to merge all the rest of the accounts into.
            var targetContactID = (from c in DataModel.Contacts
                                   join cs in DataModel.ContactStatistics on c.ID equals cs.ContactID
                                   where c.Email == targetEmail && !c.Voided && !cs.Voided && c.AccountID != null
                                   orderby cs.LastSignIn descending
                                   select c.ID).FirstOrDefault();

            return contacts.SingleOrDefault(p => p.ID == targetContactID);
        }

        #endregion FindMostSuitableContactWithAccountByEmail Method

        #region GetOrganizationContacts Method

        public List<OrganizationContact> GetOrganizationContacts(Contact contact)
        {
            return DataModel.OrganizationContacts.Where(p => p.ContactID == contact.ID && !p.Voided).ToList();
        }

        #endregion GetOrganizationContacts Method

        #region GetRegulatorContacts Method

        public List<RegulatorContact> GetRegulatorContacts(Contact contact)
        {
            return DataModel.RegulatorContacts.Where(p => p.ContactID == contact.ID && !p.Voided).ToList();
        }

        #endregion GetRegulatorContacts Method

        #region UpdateOrganizationContactMappings Method

        public void UpdateOrganizationContactMappings(Contact targetContact, Contact contact, List<OrganizationContact> targetContactMappings)
        {
            OnNotification("----> Analyzing OrganizationContact mappings for ContactID: " + contact.ID + "...");
            var organizationContacts = GetOrganizationContacts(contact);
            OnNotification("------> Found " + organizationContacts.Count + " mappings...");
            foreach (var organizationContact in organizationContacts)
            {
                if (targetContactMappings.Count(p => p.OrganizationID == organizationContact.OrganizationID) > 0)
                {
                    //the targetContact already has a mapping, so lets void this contact's OrganizationContact for this OrganizationID.
                    organizationContact.Voided = true;
                    OnNotification("--------> Voiding OrganizationContact for ContactID: " + contact.ID + " and OrganizationID: " + organizationContact.OrganizationID);
                }
                else
                {
                    //the targetContact doesn't have this mapping, so lets update the ContactID of the OrganizationContact for this Organization so we link
                    //the Organization to the targetContact.
                    organizationContact.ContactID = targetContact.ID;
                    OnNotification("--------> Re-associating OrganizationContact for ContactID: " + contact.ID + " and OrganizationID: " + organizationContact.OrganizationID + " >> ContactID: " + targetContact.ID);
                }
            }
        }

        #endregion UpdateOrganizationContactMappings Method

        #region UpdateRegulatorContactMappings Method

        public void UpdateRegulatorContactMappings(Contact targetContact, Contact contact, List<RegulatorContact> targetContactMappings)
        {
            OnNotification("----> Analyzing RegulatorContact mappings for ContactID: " + contact.ID + "...");
            var regulatorContacts = GetRegulatorContacts(contact);
            OnNotification("------> Found " + regulatorContacts.Count + " mappings...");
            foreach (var regulatorContact in regulatorContacts)
            {
                if (targetContactMappings.Count(p => p.RegulatorID == regulatorContact.RegulatorID) > 0)
                {
                    //the targetContact already has a mapping, so lets void this contact's RegulatorContact for this RegulatorID.
                    OnNotification("--------> Voiding RegulatorContact for ContactID: " + contact.ID + " and RegulatorID: " + regulatorContact.RegulatorID);
                    regulatorContact.Voided = true;
                }
                else
                {
                    //the targetContact doesn't have this mapping, so lets update the ContactID of the RegulatorContact for this Regulator so we link
                    //the Regulator to the targetContact.
                    regulatorContact.ContactID = targetContact.ID;
                    OnNotification("--------> Re-associating RegulatorContact for ContactID: " + contact.ID + " and RegulatorID: " + regulatorContact.RegulatorID + " >> ContactID: " + targetContact.ID);
                }

                if (Commit)
                {
                    DataModel.SaveChanges();
                }
            }
        }

        #endregion UpdateRegulatorContactMappings Method

        #region DeleteEvents

        private void DeleteEvents(Contact contact)
        {
            var events = DataModel.Events.Where(p => p.ContactID == contact.ID && !p.Voided).ToList();
            OnNotification("----> Deleting " + events.Count + " Event(s) for ContactID: " + contact.ID);
            foreach (var evnt in events)
            {
                evnt.Voided = true;
            }
        }

        #endregion DeleteEvents

        #endregion Helper Methods

        #region Nested Types

        public class DuplicateEmail
        {
            public string Email { get; set; }

            public int Count { get; set; }
        }

        #endregion Nested Types
    }
}