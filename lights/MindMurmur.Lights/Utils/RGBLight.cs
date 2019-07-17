using System;
using System.Threading.Tasks;
using Windows.Devices.Pwm;

namespace MindMurmur.Lights
{
    public class RGBLight : IDisposable
    {
        PwmController PWMController;

        PwmPin Rpin;
        PwmPin Gpin;
        PwmPin Bpin;

        private RGBLight()
        {

        }

        async public static Task<RGBLight> Create(int Rpin, int Gpin, int Bpin)
        {
            return null;
        }

        public void SetColor(string HexColor)
        {
            SetColor(GetColorFromHex(HexColor));
        }

        public void SetColor(System.Drawing.Color Color)
        {
            SetARGB(((int)Color.A) / 255f, ((int)Color.R) / 255f, ((int)Color.G) / 255f, ((int)Color.B) / 255f);
        }
        public void SetARGB(float A, float R, float G, float B)
        {
            if (Rpin.IsStarted)
                Rpin.Stop();
            if (Gpin.IsStarted)
                Gpin.Stop();
            if (Bpin.IsStarted)
                Bpin.Stop();
            Rpin.SetActiveDutyCyclePercentage(1 - (A * R));
            Gpin.SetActiveDutyCyclePercentage(1 - (A * G));
            Bpin.SetActiveDutyCyclePercentage(1 - (A * B));
            Rpin.Start(); Gpin.Start(); Bpin.Start();
        }

        public void Dispose()
        {
            Rpin.Stop(); Gpin.Stop(); Bpin.Stop();
            Rpin.Dispose();
            Gpin.Dispose();
            Bpin.Dispose();
        }

        public static System.Drawing.Color GetColorFromHex(string hexString)
        {
            //add default transparency to ignore exception
            if (!string.IsNullOrEmpty(hexString) && hexString.Length > 6)
            {
                if (hexString.Length == 7)
                {
                    hexString = "FF" + hexString;
                }

                hexString = hexString.Replace("#", string.Empty);
                byte a = (byte)(Convert.ToUInt32(hexString.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hexString.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hexString.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hexString.Substring(6, 2), 16));
                System.Drawing.Color color = System.Drawing.Color.FromArgb(a, r, g, b);
                return color;
            }

            //return black if hex is null or invalid
            return System.Drawing.Color.FromArgb(255, 0, 0, 0);
        }

    }
}
