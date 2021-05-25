using System;
using System.Threading.Tasks;

namespace AccessOAuthRESTApi
{
    class Program 
    {
        static async Task Main(string[] args)
        {

            HereGeoProvider _hereGeoProvider = new HereGeoProvider();

            int distance = 0;
 
            var long1 = -9.125926;
            var long2 = -9.147653;
            var lat1 = 38.75445;
            var lat2 = 38.729256;
            try
            {              
                for (int i = 0; i < 5000; i++)
                {
                    distance = await _hereGeoProvider.GetDrivingDistance(lat1.ToString(), long1.ToString(), lat2.ToString(), long1.ToString());
                    lat1 += 0.00005;
                    lat2 += 0.00005;
                    long1 += 0.00005;
                    long2 += 0.00005;
                    Console.WriteLine(distance);
                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Fim");
            Console.ReadKey();
        }
    }
}
