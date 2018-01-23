using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CERS.Validation;

namespace CERS.EDT
{
	public static class EDTValidationExtensionMethods
	{
		public static void CommitValidationResults(this IFacilitySubmittalElementValidationResult fseValidationResults, EDTTransactionScope transactionScope)
		{
			ICERSRepositoryManager repo = transactionScope.Repository;
			repo.GuidanceMessages.Update(fseValidationResults, transactionScope.Transaction.ID);
			repo.FacilitySubmittalElementResources.Update(fseValidationResults);
		}
	}
}