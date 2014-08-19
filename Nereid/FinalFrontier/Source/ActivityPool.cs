using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Nereid
{
   namespace FinalFrontier
   {
      class ActivityPool : Pool<Activity>
      {
         private static volatile ActivityPool instance;

         public static ActivityPool Instance()
         {
            if (instance == null)
            {
               instance = new ActivityPool();
               Log.Info("new activity pool instance created");
            }
            return instance;
         }

         private static readonly Mutex mutex = new Mutex();

         private ActivityPool()
         {

         }

         protected override string CodeOf(Activity x)
         {
            return x.GetCode();
         }

         public void RegisterActivity(Activity activity)
         {
            try
            {
               Log.Info("registering activity "+activity.GetCode()+": "+activity.GetName());
               mutex.WaitOne();
               Add(activity);
            }
            finally
            {
               mutex.ReleaseMutex();
            }
         }
      }
   }
}
