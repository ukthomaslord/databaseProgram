using System;
using System.Collections.Generic;

namespace databaseProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Pilot> listOfPilots = new List<Pilot>();
            List<Flight> listOfFlights = new List<Flight>();
        }
    }

    class Pilot
    {
        private int pilotNumber;
        private string name;

        public int get_pilotNumber()
        {
            return pilotNumber;
        }
        public string get_pilotName()
        {
            return name;
        }
    }
    class Flight
    {
        Pilot _pilot = new Pilot();
        private string flightNumber;
        private int pilotNumber;
        private string destination;
        public void assignPilot()
        {
            pilotNumber = _pilot.get_pilotNumber();
        }

    }
}
