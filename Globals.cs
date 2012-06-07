using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageProcessing
{
    public static class Globals
    {
        public static long tickCount=0;
        public static int n1 = 368;
        public static int n2 = 33;
        public static int n3 = 5;
        public static int n4 = 32;

        public static int[] x1 = new int[n1];  		//signal. Sampling period=5ms
        public static int[] x2 = new int[3];    		//lpf output
        public static int[] y2 = new int[n2];   		//attenuated lpf output
        public static int[] x3 = new int[2];   		 		//hpf output
        public static int[] y3 = new int[n3];   		  		//attenuated hpf output
        public static int[] der = new int[n4]; 		  		//derivative
        public static int[] integral = new int[n4]; 		//integration

        public static int signal = 0;
        public static int intWave = 0;
        public static int t1 = 0;
        public static int t2 = 0;
        public static int avgP1=512;
        public static int avgP2 = 512;
        public static int m1 = 0;
        public static int m2 = 0;
        public static long sign = 0;
        public static long inte = 0;
        public static int count = 0;

        public static long lastPeak = 0;
        public static long[] RR = new long[8];
        public static long avgPP = 1000;

        public static void lpf() 			// Low Pass Filter
        {
            x2[2] = x2[1];
            x2[1] = x2[0];
            x2[0] = (x2[1] << 1) - x2[2] + x1[0] - (x1[6] << 1) + x1[12];
            for (int i = n2 - 1; i > 0; i--)
                y2[i] = y2[i - 1];
            y2[0] = (x2[0] >> 5);

            integration();
        }

        public static void integration() 		//Moving Window Integration
        {
            for (int i = n4 - 1; i > 0; i--)
                integral[i] = integral[i - 1];

            integral[0] = -(der[n4 - 1] >> 5);

            for (int i = n4 - 1; i > 0; i--)
                der[i] = der[i - 1];

            der[0] = -y2[4] - 2 * y2[3] + 2 * y2[1] + y3[2];   //Differentiation
            der[0] = ((der[0] * der[0]) >> 3);

            integral[0] += (der[0] >> 5) + integral[1];

            threshold();
        }

        public static void threshold()
        {
            signal = y2[18];
            intWave = integral[0];
            t1 = (avgP1 >> 1) + (avgP1 >> 2);
            t2 = (avgP2 >> 1) + (avgP2 >> 2);
            count++;

            if (signal >= t1)
                sign = (sign << 1) | 1;
            else
                sign = (sign << 1);

            if (intWave >= t2)
                inte = (inte << 1) | 1;
            else
                inte = (inte << 1);

            m1 = Math.Max(m1, signal);
            m2 = Math.Max(m2, intWave);

            if (count == 32)
            {
                if ((sign & inte) > 0 && (Globals.tickCount - lastPeak) > 180)
                    peaks();

                if (m1 > t1)
                    avgP1 = (avgP1 + m1) >> 1;
                if (m2 > t2)
                    avgP2 = (avgP2 + m2) >> 1;

                count = 0;
                m1 = m2 = 0;
            }

            if (Globals.tickCount - lastPeak > 4000)
            {
                avgP1 = m1;
                avgP2 = m2;
                peaks();
            }

        }

        public static void peaks()
        {
            for (int i = 0; i < 7; i++)
                RR[i] = RR[i + 1];

            RR[7] = Globals.tickCount - lastPeak;
            lastPeak = Globals.tickCount;

            avgPP = (RR[0] + RR[1] + RR[2] + RR[3] + RR[4] + RR[5] + RR[6] + RR[7]) >> 3;

    //        lcd.print((long)60000 / avgPP);
        }

    }
}
