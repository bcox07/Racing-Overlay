using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingOverlay.Models
{
    public class Track
    {
        public Track(int id)
        {
            Id = id;
        }
        public int Id { get; set; }
        public string Name 
        {
            get
            {
                switch(Id)
                {
                    case 18:
                        return "road america";
                    case 127:
                        return "road atlanta";
                    case 144:
                        return "road mosport";
                    case 145:
                        return "brands hatch";
                    case 163:
                        return "spa";
                    case 168:
                        return "suzuka";
                    case 192:
                        return "daytona";
                    case 200:
                        return "zolder";
                    case 212:
                        return "interlagos";
                    case 219:
                        return "bathurst";
                    case 266:
                        return "imola full";
                    default:
                        return "";
                        
                };
            }
        }
        public int Length
        {
            get
            {
                switch (Id)
                {
                    case 18:
                        return 6412;
                    case 127:
                        return 4056;
                    case 144:
                        return 3914;
                    case 145:
                        return 3884;
                    case 163:
                        return 6930;
                    case 168:
                        return 5752;
                    case 192:
                        return 5686;
                    case 200:
                        return 3934;
                    case 212:
                        return 4222;
                    case 219:
                        return 6144;
                    case 266:
                        return 4862;
                    default:
                        return -1;

                }
                ;
            }
        }
    }
}
