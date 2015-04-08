﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using PriorityQueue;

namespace TSP
{
    class BranchAndBound
    {
        private City[] Cities;
        private ArrayList Route;
        private TSPSolution bssf; 
        private PriorityQueue<Double,BranchAndBoundState> agenda;

        private bool timeIsUp;

        public BranchAndBound( City[] cities)
        {
            this.Cities = cities;
            Route = new ArrayList();
            bssf = new TSPSolution(Route);
            agenda = new PriorityQueue<double, BranchAndBoundState>();
            timeIsUp = false;
        }

        public TSPSolution GetBSSF() 
        {
            return bssf;
        }

        // greedy algorithm implementation for first bssf solution
        private TSPSolution GenerateQuickSolution()
        {
            // initialize data structures
            Route = new ArrayList();
            HashSet<City> citiesLeft = new HashSet<City>(Cities);
            Route.Add(Cities[0]);
            citiesLeft.Remove(Cities[0]);
            

            while (citiesLeft.Count != 0)
            {
                City currentCity = Route[Route.Count - 1] as City;
                double bestCost = Double.PositiveInfinity;
                City bestCity = null;

                // go through cities left to find the best cost
                foreach (City city in citiesLeft)
                {
                    double cost = currentCity.costToGetTo(city);
                    if (cost < bestCost)
                    {
                        bestCity = city;
                        bestCost = cost;
                    }
                }

                // I don't think it will be null, but just incase...
                if (bestCity == null)
                {
                    throw new System.ArgumentNullException();
                }

                Route.Add(bestCity);
                citiesLeft.Remove(bestCity);


            }

            return new TSPSolution(Route);
        }

        private BranchAndBoundState init_state() {
            return null;
        }
        
        public TSPSolution AnalyzePath( int max_running_time ) {

            BranchAndBoundState state = new BranchAndBoundState();

            bssf = GenerateQuickSolution();
            
            agenda.Clear();

            agenda.Enqueue( state.GetBound(), state );

            while( !agenda.IsEmpty && !this.timeIsUp && bssf.GetCost() != agenda.Peek().Value.GetBound() )
            {
                BranchAndBoundState u = agenda.Dequeue().Value;

                if( u.GetBound() < bssf.GetCost()) {
                    ArrayList successors = u.GetSuccessors();

                    foreach( BranchAndBoundState w in successors ) 
                    {
                        if( this.timeIsUp ) break; 
                        
                        if( w.GetBound() < bssf.GetCost())
                        {
                            if( w.IsSolution() ) 
                            {
                                bssf = new TSPSolution(w.GetCities());
                            }
                            else
                            {
                                agenda.Enqueue(w.GetBound(), w);
                            }
                        }
                    }
                }
            }

            return bssf;

        }

    }

    public class BranchAndBoundState
    {
        Double bound;
        ArrayList Cities;
        
        public Double GetBound() {
            return bound;
        }

        public ArrayList GetSuccessors() {
            ArrayList successors = new ArrayList();
            return successors;
        }

        // aka Criterion
        public bool IsSolution() {
            return false;
        }

        public ArrayList GetCities()
        {
            return Cities;
        }

    }
    
    // Base code for this class provided with CS312 TSP project
    public class TSPSolution : BranchAndBoundState
    {
        /// <summary>
        /// we use the representation [cityB,cityA,cityC] 
        /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
        /// and the edge from cityC to cityB is the final edge in the path.  
        /// you are, of course, free to use a different representation if it would be more convenient or efficient 
        /// for your node data structure and search algorithm. 
        /// </summary>
        public ArrayList Route;
        private Double cost;

        public TSPSolution(ArrayList iroute)
        {
            Route = new ArrayList(iroute);
            cost = GetCost(); 
        }


        /// <summary>
        ///  compute the cost of the current route.  does not check that the route is complete, btw.
        /// assumes that the route passes from the last city back to the first city. 
        /// </summary>
        /// <returns></returns>
        public double GetCost()
        {
            // If we've already done the work, just return it
            if( this.cost > -1 )
                return this.cost;

            // go through each edge in the route and add up the cost. 
            int x;
            City here; 
            double cost = 0D;
                
            for (x = 0; x < Route.Count-1; x++)
            {
                here = Route[x] as City;
                cost += here.costToGetTo(Route[x + 1] as City);
            }
            // go from the last city to the first. 
            here = Route[Route.Count - 1] as City;
            cost += here.costToGetTo(Route[0] as City);
            return cost; 
        }
    }
}
