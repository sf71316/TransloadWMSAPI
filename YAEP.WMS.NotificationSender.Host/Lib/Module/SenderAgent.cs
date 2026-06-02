using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Constants;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.NotificationReceiver.Common;
using YAEP.WMS.NotificationSender.Client;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    public class SenderAgent
    {
        const int MAX_RETRY_COUNT = 5;//通知排程最大嘗式次數
        const string LOG_TYPE = "Notify Message";
        const string LOGGER = "Sender Agent";
        public void ExecuteAsync()
        {
            var logger = Logger.GetLogger();
            NotificationSenderTaskRepository repository = new NotificationSenderTaskRepository();
            try
            {


                this.OnNotify("Start check notification....");
                var task = repository.GetProcessTasks(MAX_RETRY_COUNT);
                List<Task> tasks = new List<Task>();
                TaskScheduler.UnobservedTaskException += (o, e) =>
                {
                    e.SetObserved();
                    Console.WriteLine($"Task unobservedTaskExeption occurs {e.Exception.StackTrace}.");
                    Console.WriteLine($"Task unobservedTaskExeption occurs {e.Exception.Message}.");
                };
                foreach (var t in task)
                {
                    SenderConfigure config = new SenderConfigure();
                    config.ReceiverSecret = t.ReceiverSecret;
                    config.ReceiverUrl = t.ReceiverUrl;
                    var api = AbstractSenderAPI.GetSenderAPI(config);
                    api.RetryCount = MAX_RETRY_COUNT;
                    NotificationRequest request = new NotificationRequest();
                    request.EventName = t.EventName;
                    request.Data = t.Message;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var response = api.SendNotify(request);
                    sw.Stop();
                    this.OnNotify($"Receivce RefNo #{t.RefNo} message to {t.ReceiverUrl} {(response.IsComplete ? "success" : "failure")}.....elapsed {sw.ElapsedMilliseconds}ms",
                        response.IsComplete ? ConsoleColor.Green : ConsoleColor.Red);
                    if (response.Data) //訊息送出成功
                    {
                        t.Status = (int)SenderTaskStatus.Complete;
                        repository.Update(t, new { UID = t.UID });
                    }
                    else //訊息送出失敗
                    {
                        logger.Log($"Notify Failure:{response.Message}", LOG_TYPE, LOGGER, "error",
                            (int)BelongToTypes.WMSCommon, application: WMSAPIParameters.APPLICATION_NAME, belongToUID: t.RefNo);
                        t.Status = (int)SenderTaskStatus.Failure;
                        t.RetryCount++;
                        repository.Update(t, new { UID = t.UID });
                    }
                    //  tasks.Add(Task.Run(() =>
                    //{

                    //}));

                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                this.OnNotify(ex.Message);
            }
            this.OnNotify("Finish check notification....");
        }
        protected void OnNotify(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            NotificationArg arg = new NotificationArg();
            arg.Message = message;
            arg.Color = color;
            if (this.Notify != null)
            {
                this.Notify(this, arg);
            }
        }
        public event EventHandler<NotificationArg> Notify;
    }
}
