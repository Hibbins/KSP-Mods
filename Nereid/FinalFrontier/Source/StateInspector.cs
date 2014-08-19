using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace FinalFrontier
   {
      // class for inspecting simple states, such as change in vessel altitute, mach number  or change in Gee-Force
      public abstract class StateInspector<T>
      {
         private volatile bool changed = false;

         public abstract void Inspect(T x);

         public void Clear()
         {
            this.changed = false;
         }

         public void Reset()
         {
            Clear();
            ResetState();
         }

         protected abstract void ResetState();

         public void Change()
         {
            this.changed = true;
         }

         public bool StateHasChanged()
         {
            return this.changed;
         }
      }

      public class MachNumberInspector : StateInspector<Vessel>
      {
         private int lastMachNumber = 0;

         public override void Inspect(Vessel vessel)
         {
            if (vessel == null) return;
            if(vessel.situation != Vessel.Situations.FLYING) return;

            double mach = vessel.MachNumber();
            int machWholeNumber = (int)Math.Truncate(mach);
            if (machWholeNumber > lastMachNumber)
            {
               Log.Info("mach number increasing to " + machWholeNumber);
               this.lastMachNumber = machWholeNumber;
               Change();
            }
            else if (machWholeNumber < lastMachNumber)
            {
               Log.Info("mach number decreasing to " + machWholeNumber);
               this.lastMachNumber = machWholeNumber;
               Change();
            }
         }

         protected override void ResetState()
         {
            this.lastMachNumber = 0;
         }
      }

      public class AltitudeInspector : StateInspector<Vessel>
      {
         private long lastAltitudeAsMultipleOf1k = 0;

         public override void Inspect(Vessel vessel)
         {
            if (vessel == null) return;
            if (vessel.situation != Vessel.Situations.FLYING) return;

            double altitide = vessel.altitude;
            int alt1000k = 1000*(int)Math.Truncate(altitide/1000);
            if (alt1000k > lastAltitudeAsMultipleOf1k)
            {
               if(Log.IsLogable(Log.LEVEL.DETAIL))  Log.Detail("altitude increasing to " + alt1000k);
               this.lastAltitudeAsMultipleOf1k = alt1000k;
               Change();
            }
            else if (alt1000k < lastAltitudeAsMultipleOf1k)
            {
               if(Log.IsLogable(Log.LEVEL.DETAIL))  Log.Detail("altitude decreasing to " + alt1000k);
               this.lastAltitudeAsMultipleOf1k = alt1000k;
               Change();
            }
         }

         protected override void ResetState()
         {

         }
      }

      public class GeeForceInspector : StateInspector<Vessel>
      {
         public const double DURATION = 3.0;
         private const int MAX_GEE = 15;
         private double maxGeeForce = 1.0;
         private int gSustained = 1;
         private double[] gTimeOf = new double[MAX_GEE];

         public GeeForceInspector()
         {
            ResetState();
         }


         public override void Inspect(Vessel vessel)
         {
            double gForce = vessel.geeForce;
            if (gForce > maxGeeForce)
            {
               if(Log.IsLogable(Log.LEVEL.DETAIL)) Log.Detail("max gee force increased to " + gForce);
               this.maxGeeForce = gForce;
               int g = (int)gForce;
               double now = Planetarium.GetUniversalTime();
               if (g < MAX_GEE)
               {
                  if(gTimeOf[g] <= 0)
                  {
                     if (Log.IsLogable(Log.LEVEL.DETAIL)) Log.Detail("gee force " + g + " reached at " + now);
                     for (int i = 0; i < g; i++ )
                     {
                        if (gTimeOf[i] <= 0) gTimeOf[i] = now;
                     }
                     gTimeOf[g] = now;
                  }
               }


               for(int i=2; i<g && i<MAX_GEE; i++)
               {
                  if (Log.IsLogable(Log.LEVEL.TRACE)) Log.Trace("check g " + i + " since " + gTimeOf[i] + " at " + now);
                  if (gTimeOf[i] <= 0) break;
                  if (gTimeOf[i] + DURATION > now)
                  {
                     if(Log.IsLogable(Log.LEVEL.DETAIL)) Log.Detail("g " + i+" not reached");
                     return;
                  }
                  else
                  {
                     if (Log.IsLogable(Log.LEVEL.DETAIL)) Log.Detail("g " + i + " sustained for " + (now - gTimeOf[i]) + " seconds");
                     gSustained = i;
                     Change();
                  }
               }
            }
         }

         public int GetGeeNumber()
         {
            return gSustained;
         }


         protected override void ResetState()
         {
            Log.Detail("reset of gee inspector state");
            this.maxGeeForce = 1.0;
            this.gSustained = 1;
            for(int i=0; i<MAX_GEE; i++)
            {
               gTimeOf[i] = 0.0;
            }
         }
      }

      public class AtmosphereInspector : StateInspector<Vessel>
      {
         private bool inAtmosphere = true;

         public override void Inspect(Vessel vessel)
         {
            bool inAtmosphere = vessel.IsInAtmosphere();
            if (this.inAtmosphere != inAtmosphere)
            {
               this.inAtmosphere = inAtmosphere;
               Change();
            }
         }

         protected override void ResetState()
         {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if(vessel!=null)
            {
               inAtmosphere = vessel.IsInAtmosphere();
            }
            else
            {
               inAtmosphere = false;
            }
         }
      }

   } // end of FinalFrontier namespace */
}
