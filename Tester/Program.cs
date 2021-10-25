﻿using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ServiceClient x = Helper.Service();
            Console.WriteLine(x.IsReady);
            DeleteAllRecords(x);
            //KeyVaultService _service = new KeyVaultService();
            //string secretValue= await _service.GetSecretValue("D365ConnectionString");
            // Console.WriteLine($"Secret: {secretValue}");
        }
        private static void DeleteAllRecords(ServiceClient x)
        {
            Console.WriteLine("Deleting all records. Please wait...");

            BulkDeleteRequest request = new BulkDeleteRequest
            {
                JobName = "Delete All",
                ToRecipients = new Guid[] { },
                CCRecipients = new Guid[] { },
                RecurrencePattern = string.Empty,
                QuerySet = new QueryExpression[]
                {
            new QueryExpression { EntityName = "account" },
            new QueryExpression { EntityName = "contact" }
            //new QueryExpression { EntityName = "lead" }
                }
            };

            BulkDeleteResponse response = (BulkDeleteResponse)x.Execute(request);
            Guid jobId = response.JobId;

            bool deleting = true;

            while (deleting)
            {
                Console.WriteLine("still deleting");
                Thread.Sleep(10000);    // poll crm every 10 seconds 

                QueryExpression query = new QueryExpression { EntityName = "bulkdeleteoperation" };
                query.Criteria.AddCondition("asyncoperationid", ConditionOperator.Equal, jobId);
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 3);
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 30);

                EntityCollection results = x.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    Console.WriteLine("finished deleting");
                    deleting = false;
                }
            }
        }
    }
}
