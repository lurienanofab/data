using LNF;
using LNF.Billing.Reports;
using LNF.CommonTools;
using LNF.Data;
using LNF.DataAccess;
using LNF.Impl;
using LNF.Impl.Billing;
using LNF.Impl.Repository.Data;
using LNF.PhysicalAccess;
using LNF.Scripting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Data.Models.CommandLine
{
    public static class CommandLineUtility
    {
        private static readonly ScriptHost host;
        private static readonly List<ScriptHost.Command> commands;

        public static IProvider Provider => Startup.WebApp.Context.GetInstance<IProvider>();
        public static ISession DataSession => Provider.DataAccess.Session;

        static CommandLineUtility()
        {
            host = new ScriptHost();

            commands = new List<ScriptHost.Command>
            {
                ScriptHost.Command.Create(
                name: "help",
                syntax: "help()",
                args: new string[] { "none" },
                example: "help()",
                helpSummary: "displays a list of commands",
                helpDetail: "Displays a list of each command with a short description.",
                method: (Func<string>)HelpSummary),

                ScriptHost.Command.Create(
                name: "more",
                syntax: "more(name)",
                args: new string[] { "name: The command name (without parenthesis)." },
                example: "more(\"help\")",
                helpSummary: "displays a detailed help message",
                helpDetail: "Displays detailed information about a command.",
                method: (Func<string, string>)HelpDetail),

                ScriptHost.Command.Create(
                name: "now",
                syntax: "now()",
                args: new string[] { "none" },
                example: "now()",
                helpSummary: "displays the current server time",
                helpDetail: "Displays the current time according to the server. Can be used to check the difference between server and client times.",
                method: (Func<DateTime>)(() => DateTime.Now)),

                ScriptHost.Command.Create(
                name: "clients",
                syntax: "clients(search)",
                args: new string[] { "search: An object for which to search. Numbers will search for a matching ClientID, strings will search for any ccurrence in username, first, last, or middle names." },
                example: "clients(\"dgrimard\")",
                helpSummary: "displays a collection of clients found by the search term",
                helpDetail: "Searches all clients and returns a list.",
                method: (Func<object, ClientInfoCollection>)CommandLineUtility.ClientInfo),

                ScriptHost.Command.Create(
                name: "monthly_financial_emails",
                syntax: "monthly_financial_emails(date, args)",
                args: new string[] { "date: The string representation of a period for which to send emails.", "args: A dictionary composed of IncludeManager (bool), Recipients (a list of emails), Message (special text to include in the email). The recipients list should be a list object, for example: [\"email1@example.com\", \"email2@example.com\"]" },
                example: "monthly_financial_emails(\"2014-07-01\", {\"IncludeManager\": false, \"Recipients\": [\"your@email.com\"], \"Message\": \"this is a test\"})",
                helpSummary: "sends the monthly emails to financial managers",
                helpDetail: "Emails are sent to financial managers at the beginning of the month. The email text includes a list of all users under the manager's accounts who had charges during the month. This method allows re-sending emails in the event a problem occurs or for testing.",
                method: (Func<string, IDictionary<object, object>, ScriptHost.Result>)CommandLineUtility.MonthlyFinancialEmails)
            };

            //Func<object, ScriptHost.Result> accountInfo = CommandLineUtility.AccountInfo;
            //Func<int, ScriptHost.Result> prowatchCreateUser = CommandLineUtility.ProwatchCreateUser;
            //Func<string, ScriptHost.Result> schedulerTask = CommandLineUtility.SchedulerTask;
            //Func<string, ScriptHost.Result> taskCheck = CommandLineUtility.TaskCheck;

            //Func<string, IDictionary<object, object>, ScriptHost.Result> billingTask = (task, dict) =>
            //{
            //    QueryParameters queryParams = QueryParameters.Create(dict);
            //    return CommandLineUtility.BillingTask(task, queryParams);
            //};

            //Func<string, IDictionary<object, object>, ScriptHost.Result> monthlyFinancialEmails = (date, dict) =>
            //{
            //    DateTime d = DateTime.Parse(date);
            //    QueryParameters queryParams = QueryParameters.Create(dict);
            //    return CommandLineUtility.MonthlyFinancialEmails(d, queryParams);
            //};

            //Func<int, ScriptHost.Result> resetPassword = CommandLineUtility.ResetPassword;


            foreach (ScriptHost.Command c in commands)
                host.Register(c.Name, c.Method);

            //host.Register("client_info", clientInfo);
            //host.Register("account_info", accountInfo);
            //host.Register("prowatch_create_user", prowatchCreateUser);
            //host.Register("scheduler_task", schedulerTask);
            //host.Register("billing_task", billingTask);
            //host.Register("reset_password", resetPassword);
            //host.Register("task_check", taskCheck);
            //host.Register("", monthlyFinancialEmails);
        }

        public static ScriptHost.Command[] GetCommands()
        {
            return commands.ToArray();
        }

        public static string HelpSummary()
        {
            string result = string.Empty;

            ScriptHost.Command[] list = CommandLineUtility.GetCommands();
            if (list.Length > 0)
            {
                foreach (var c in list)
                    result += c.Syntax + ": " + c.HelpSummary + " [example: " + c.Example + "]" + "\n";
            }
            else
                result = "no commands found";

            return result;
        }

        public static string HelpDetail(string cmd)
        {
            string result = string.Empty;

            ScriptHost.Command c = CommandLineUtility.GetCommands().FirstOrDefault(x => x.Name == cmd.ToString());
            if (c != null)
                result = c.HelpDetail + "\n\nSyntax:<ul><li>" + c.Syntax + "</li></ul>Arguments:<ul><li>" + string.Join("</li><li>", c.Arguments) + "</li></ul>Example:<ul><li>" + c.Example + "</li></ul>\n";
            else
                result = string.Format("command {0} not found", cmd);

            return result;
        }

        public static ScriptHost.Result Execute(string cmd)
        {
            ScriptHost.Result scriptResult;

            try
            {
                scriptResult = host.Run(cmd);
            }
            catch (Exception ex)
            {
                scriptResult = new ScriptHost.Result()
                {
                    Success = false,
                    Message = ex.Message + "\nUse help() for a list of commands",
                    Data = null
                };
            }

            return scriptResult;
        }

        public static ScriptHost.Result ResetPassword(int id)
        {
            ScriptHost.Result result = new ScriptHost.Result();

            Client c = DataSession.Single<Client>(id);

            if (c != null)
            {
                Provider.Data.Client.AuthUtility().ResetPassword(c.ClientID);
                result.Success = true;
                result.Message = string.Format("{0} password reset.", c.UserName);
            }
            else
            {
                result.Success = false;
                result.Message = string.Format("ClientID {0} not found", id);
            }

            return result;
        }

        public static ScriptHost.Result BillingTask(string task, QueryParameters queryParams)
        {
            ScriptHost.Result result = new ScriptHost.Result();

            DateTime sd;
            DateTime ed;
            DateTime period;

            void getDates(bool startRequired, bool endRequired, bool periodRequired)
            {
                if (startRequired && !queryParams.ContainsParameter("StartDate"))
                    throw new Exception("Missing parameter: StartDate");

                if (endRequired && !queryParams.ContainsParameter("EndDate"))
                    throw new Exception("Missing parameter: EndDate");

                if (periodRequired && !queryParams.ContainsParameter("Period"))
                    throw new Exception("Missing parameter: Period");

                sd = queryParams.GetValue("StartDate", DateTime.Now);
                ed = queryParams.GetValue("EndDate", DateTime.Now);
                period = queryParams.GetValue("Period", DateTime.Now);
            }

            var context = "Data.Models.CommandLine.CommandLineUtility.BillingTask";
            var clientId = queryParams.GetValue("ClientID", 0);

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();
                switch (task)
                {
                    case "ToolDataClean":
                        getDates(true, true, false);
                        var writeToolDataCleanResult = new WriteToolDataCleanProcess(new WriteToolDataCleanConfig { Connection = conn, StartDate = sd, EndDate = ed, ClientID = clientId, Context = context }).Start();
                        result.Success = true;
                        result.Message = null;
                        result.Data = writeToolDataCleanResult.LogText;
                        break;
                    case "ToolData":
                        getDates(true, true, false);
                        var writeToolDataResult = new WriteToolDataProcess(new WriteToolDataConfig { Connection = conn, Period = sd, ClientID = clientId, ResourceID = queryParams.GetValue("ResourceID", 0), Context = context }).Start();
                        result.Success = true;
                        result.Message = null;
                        result.Data = writeToolDataResult.LogText;
                        break;
                    case "RoomDataClean":
                        getDates(true, true, false);
                        var writeRoomDataCleanResult = new WriteRoomDataCleanProcess(new WriteRoomDataCleanConfig { Connection = conn, StartDate = sd, EndDate = ed, ClientID = clientId, Context = context }).Start();
                        result.Success = true;
                        result.Message = null;
                        result.Data = writeRoomDataCleanResult.LogText;
                        break;
                    case "RoomData":
                        getDates(true, true, false);
                        var writeRoomDataResult = new WriteRoomDataProcess(new WriteRoomDataConfig { Connection = conn, Period = sd, ClientID = clientId, RoomID = queryParams.GetValue("RoomID", 0), Context = context }).Start();
                        result.Success = true;
                        result.Message = null;
                        result.Data = writeRoomDataResult.LogText;
                        break;
                    case "StoreDataClean":
                        getDates(true, true, false);
                        var writeStoreDataCleanResult = new WriteStoreDataCleanProcess(new WriteStoreDataCleanConfig { Connection = conn, StartDate = sd, EndDate = ed, ClientID = clientId, Context = context }).Start();
                        result.Success = true;
                        result.Message = null;
                        result.Data = writeStoreDataCleanResult.LogText;
                        break;
                    case "StoreData":
                        getDates(true, true, false);
                        var writeStoreDataResult = new WriteStoreDataProcess(new WriteStoreDataConfig { Connection = conn, Period = sd, ClientID = clientId, ItemID = queryParams.GetValue("ItemID", 0), Context = context }).Start();
                        result.Success = true;
                        result.Message = null;
                        result.Data = writeStoreDataResult.LogText;
                        break;
                    case "ToolBillingStep1":
                        getDates(false, false, true);
                        var step1 = new BillingDataProcessStep1(new Step1Config { Connection = conn, Period = period, ClientID = clientId, IsTemp = queryParams.GetValue("IsTemp", false), Now = DateTime.Now, Context = context });
                        var step1Result = step1.PopulateToolBilling();
                        result.Success = true;
                        result.Message = null;
                        result.Data = step1Result.LogText;
                        break;
                    default:
                        result.Success = false;
                        result.Message = "Unknown task: " + task;
                        break;
                }
                conn.Close();
            }

            return result;
        }

        public static ScriptHost.Result SchedulerTask(string task, IClient currentUser)
        {
            ScriptHost.Result result = new ScriptHost.Result();
            var model = new ServiceModel() { Task = task, CurrentUser = currentUser };
            GenericResult gr = model.HandleCommand();

            if (gr.Success)
            {
                result.Success = true;
                result.Message = "ok";
            }
            else
            {
                result.Success = false;
                result.Message = "Service did not respond.";
                result.Data = new string[] { task };
            }

            return result;
        }

        public static ScriptHost.Result ProwatchCreateUser(int id)
        {
            ScriptHost.Result result = new ScriptHost.Result();

            var c = DataSession.Single<LNF.Impl.Repository.Data.ClientInfo>(id).CreateModel<IClient>();

            if (c == null)
            {
                result.Success = false;
                result.Message = string.Format("ClientID {0} not found", id);
            }
            else
            {
                Badge b = Provider.PhysicalAccess.GetBadge(c.ClientID).FirstOrDefault();

                if (b == null)
                {
                    Provider.PhysicalAccess.AddClient(new AddClientRequest { ClientID = c.ClientID, UserName = c.UserName, FName = c.FName, LName = c.LName, MName = c.MName });
                    result.Success = true;
                    result.Message = "ok";
                }
                else
                {
                    result.Success = false;
                    result.Message = string.Format("ClientID {0} already exists", id);
                }
            }

            return result;
        }

        public static ScriptHost.Result AccountInfo(object search)
        {
            ScriptHost.Result result = new ScriptHost.Result();

            IEnumerable<AccountInfo> c = null;

            if (search is int)
            {
                var single = DataSession.Single<Account>(Convert.ToInt32(search));

                if (single != null)
                {
                    c = new Account[] { single }.Select(x => new AccountInfo()
                    {
                        AccountID = x.AccountID,
                        AccountName = x.Name,
                        ShortCode = x.ShortCode
                    });
                }
            }
            else
            {
                string s = search.ToString();
                List<Account> query = new List<Account>();

                query.AddRange(DataSession.Query<Account>().Where(x => x.Name.Contains(s)));
                query.AddRange(DataSession.Query<Account>().Where(x => x.ShortCode.Contains(s)));

                c = query.Select(x => new AccountInfo()
                {
                    AccountID = x.AccountID,
                    AccountName = x.Name,
                    ShortCode = x.ShortCode
                }).ToList();
            }

            if (c == null || c.Count() == 0)
            {
                result.Success = false;
                result.Message = string.Format("Accounts not found using {0}", search);
                result.Data = null;
            }
            else
            {
                result.Success = true;
                result.Message = string.Format("Found {0} accounts\n", c.Count());
                result.Data = c;
            }

            return result;
        }

        public static ReservationInfoCollection ReservationInfo(object search)
        {
            throw new NotImplementedException();
        }

        public static ClientInfoCollection ClientInfo(object search)
        {
            IList<ClientInfo> c = null;

            if (search is int)
            {
                var single = DataSession.Single<Client>(Convert.ToInt32(search));

                if (single != null)
                {
                    c = new Client[] { single }.Select(x => new ClientInfo()
                    {
                        ClientID = x.ClientID,
                        UserName = x.UserName,
                        LName = x.LName,
                        FName = x.FName,
                        MName = x.MName
                    }).ToList();
                }
            }
            else
            {
                string s = search.ToString();
                IList<Client> query = DataSession.Query<Client>().Where(x => x.UserName.Contains(s) || x.LName.Contains(s) || x.FName.Contains(s)).ToList();

                c = query.Select(x => new ClientInfo()
                {
                    ClientID = x.ClientID,
                    UserName = x.UserName,
                    LName = x.LName,
                    FName = x.FName,
                    MName = x.MName
                }).ToList();
            }

            return new ClientInfoCollection(c);
        }

        public static ScriptHost.Result TaskCheck(string task)
        {
            ScriptHost.Result result = new ScriptHost.Result();

            string sql;

            switch (task)
            {
                case "daily":
                    sql = "SELECT 'ToolDataClean' AS 'TableName', CONVERT(nvarchar(20), MAX(ActualBeginDateTime), 120) AS 'Value' FROM sselData.dbo.ToolDataClean"
                        + " UNION SELECT 'ToolData', CONVERT(nvarchar(20), MAX(ActDate), 120) FROM sselData.dbo.ToolData"
                        + " UNION SELECT 'ToolBillingTemp', CONVERT(nvarchar(20), MAX(ActDate), 120) FROM sselData.dbo.ToolBillingTemp"
                        + " UNION SELECT 'RoomDataClean', CONVERT(nvarchar(20), MAX(EntryDT), 120) FROM sselData.dbo.RoomDataClean"
                        + " UNION SELECT 'RoomData', CONVERT(nvarchar(20), MAX(EvtDate), 120) FROM sselData.dbo.RoomData"
                        + " UNION SELECT 'RoomBillingTemp', CONVERT(nvarchar(20), MAX(Period), 120) FROM sselData.dbo.RoomBillingTemp";
                    break;
                case "monthly":
                    sql = "SELECT 'ToolDataClean' AS 'TableName', MAX(ActualBeginDateTime) AS 'MaxDate' FROM sselData.dbo.ToolDataClean"
                        + " UNION SELECT 'ToolData', MAX(ActDate) FROM sselData.dbo.ToolData"
                        + " UNION SELECT 'ToolBillingTemp', MAX(ActDate) FROM sselData.dbo.ToolBillingTemp"
                        + " UNION SELECT 'RoomDataClean', MAX(EntryDT) FROM sselData.dbo.RoomDataClean"
                        + " UNION SELECT 'RoomData', MAX(EvtDate) FROM sselData.dbo.RoomData"
                        + " UNION SELECT 'RoomBillingTemp', MAX(Period) FROM sselData.dbo.RoomBillingTemp";
                    break;
                default:
                    result.Success = false;
                    result.Message = string.Format("Invalid task: {0}", task);
                    result.Data = null;
                    return result;
            }

            var query = DataSession.SqlQuery(sql).List<TaskCheck>();

            result.Success = true;
            result.Message = null;
            result.Data = query;

            return result;
        }

        public static ScriptHost.Result MonthlyFinancialEmails(string date, IDictionary<object, object> args)
        {
            ScriptHost.Result result = new ScriptHost.Result();

            QueryParameters queryParams = QueryParameters.Create(args);

            DateTime d = DateTime.Parse(date);

            var opt = new FinancialManagerReportOptions
            {
                ClientID = 0,
                ManagerOrgID = 0,
                Period = d,
                IncludeManager = queryParams.GetValue("IncludeManager", true),
                Message = queryParams.GetValue("Message", string.Empty)
            };

            var processResult = Provider.Billing.Report.SendFinancialManagerReport(opt);
            var count = processResult.TotalEmailsSent;

            result.Success = true;
            result.Message = string.Format("sent {0} emails", count);
            result.Data = null;

            return result;
        }
    }
}