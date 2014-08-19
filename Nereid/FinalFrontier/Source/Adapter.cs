using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
//using System.Runtime.InteropServices;

namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class Adapter
      {
         private bool isInstalled  = false;

         public bool IsInstalled()
         {
            return this.isInstalled;
         }

         protected void SetInstalled(bool installed)
         {
            this.isInstalled = installed;
         }

         protected Type GetType(String name)
         {
            return AssemblyLoader.loadedAssemblies
               .SelectMany(x => x.assembly.GetExportedTypes())
               .SingleOrDefault(t => t.FullName == name);
         }

         protected bool IsTypeLoaded(String name)
         {
            return GetType(name) != null;
         }

         public abstract void Plugin();
      }

      public class FARAdapter : Adapter
      {
         private MethodInfo methodGetMachNumber;

         public override void Plugin()
         {
            Type type = GetType("ferram4.FARAeroUtil");
            if (type != null)
            {
               methodGetMachNumber = type.GetMethod("GetMachNumber", BindingFlags.Public | BindingFlags.Static);
               if(methodGetMachNumber!=null)
               {
                  SetInstalled(true);
               }
            }
         }

         public double GetMachNumber(CelestialBody body, double altitude, Vector3 velocity)
         {
            if(IsInstalled())
            {
               return (double)methodGetMachNumber.Invoke(null, new object[] { body, altitude, velocity });
            }
            return 0.0;
         }
      }

   }
}
