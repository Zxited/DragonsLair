﻿using System;
using System.Collections.Generic;
using System.Linq;
using TournamentLib;

namespace DragonsLair
{
    public class Controller
    {
        private TournamentRepo tournamentRepository = new TournamentRepo();
        Random rng = new Random();

        public void ShowScore(string tournamentName)
        {
            List<Team> winnerTeams = new List<Team>();
            Tournament t = tournamentRepository.GetTournament(tournamentName);
            List<Team> Teams = t.GetTeams();
            string[,] placement = new string[Teams.Count, 2];
            for (int i = 0; i < Teams.Count; i++)
            {
                placement[i, 0] = Teams[i].Name;
                placement[i, 1] = "0";
            }
            int rounds = t.GetNumberOfRounds();
            for (int i = 0; i < rounds; i++)
            {
                Round currentRound = t.GetRound(i);
                List<Team> winningTeams = currentRound.GetWinningTeams();
                winnerTeams.AddRange(winningTeams);
            }

            foreach (var item in winnerTeams)
            {
                for (int i = 0; i < placement.GetLength(0); i++)
                {
                    if (item.Name == placement[i, 0])
                    {
                        int.TryParse(placement[i, 1], out int temp);
                        temp++;
                        placement[i, 1] = temp.ToString();
                    }
                }
            }

            for (int i = 0; i < placement.GetLength(0); i++)
            {
                for (int j = 0; j < placement.GetLength(0); j++)
                {
                    if (int.Parse(placement[i, 1]) >= int.Parse(placement[j, 1]))
                    {
                        string[] temp1 = { "", "" };
                        string[] temp2 = { "", "" };

                        temp1[0] = placement[j, 0];
                        temp1[1] = placement[j, 1];
                        temp2[0] = placement[i, 0];
                        temp2[1] = placement[i, 1];

                        placement[i, 0] = temp1[0];
                        placement[i, 1] = temp1[1];
                        placement[j, 0] = temp2[0];
                        placement[j, 1] = temp2[1];
                    }
                }
            }

            for (int i = 0; i < placement.GetLength(0); i++)
            {
                Console.WriteLine(placement[i, 0] + " Won " + placement[i, 1] + " times");
            }
            Console.ReadKey(true);
            Console.Clear();
        }

        public void ScheduleNewRound(string tournamentName, bool printNewMatches = true, bool scrambleTeams = true)
        {
            Tournament t = tournamentRepository.GetTournament(tournamentName);
            Round lastRound, newRound;
            bool isRoundFinished;
            List<Team> teams, scrambled;
            Team oldFreeRider, newFreeRider;
            int numberOfRounds = t.GetNumberOfRounds();
            if (numberOfRounds == 0)
            {
                lastRound = null;
                isRoundFinished = true;
            }
            else
            {
                lastRound = t.GetRound(numberOfRounds - 1);
                isRoundFinished = lastRound.IsMatchesFinished();
            }

            if (isRoundFinished == true)
            {
                if (lastRound == null)
                {
                    teams = t.GetTeams();
                }
                else
                {
                    teams = lastRound.GetWinningTeams();

                    if (lastRound.FreeRider != null)
                    {
                        teams.Add(lastRound.FreeRider);
                    }
                }
                if (teams.Count >= 2)
                {
                    newRound = new Round();
                    scrambled = ScrambleTeamsRandomly(teams, scrambleTeams);
                    
                    

                    if (scrambled.Count % 2 != 0)
                    {
                        if (numberOfRounds > 0)
                        {
                            oldFreeRider = lastRound.GetFreeRider();
                            newFreeRider = scrambled[0];

                        }
                        else
                        {
                            oldFreeRider = null;
                            newFreeRider = scrambled[0];
                        }
                        int x = 0;
                        while (newFreeRider == oldFreeRider)
                        {
                            newFreeRider = scrambled[x];
                                x++;
                        }
                        newRound.SetFreeRider(newFreeRider);
                        scrambled.Remove(newFreeRider);

                   }
                    for(int i = 0; i < scrambled.Count; i += 2)
                    {
                        Match match = new Match();
                        match.FirstOpponent = scrambled[i];
                        match.SecondOpponent = scrambled[i + 1];
                        newRound.AddMatch(match);
                    }
                    t.AddRound(newRound);
                    //Vis kampe her
                }
                else
                {
                    throw new Exception("Round is not finished");
                }
            }
            else
            {
                throw new Exception("Round not finished");
            }

        }

        public void SaveMatch(string tournamentName, int roundNo, string winningTeamName)
        {
            Tournament t = tournamentRepository.GetTournament(tournamentName);
            Round r = t.GetRound(roundNo);
            Match m = r.GetMatch(winningTeamName);

            if (m != null && m.Winner == null)
            {
                Team w = t.GetTeam(winningTeamName);
                m.Winner = w;

                Console.WriteLine("Matched Saved");
            }
            else throw new Exception("Failed to save the match");
        }

        public TournamentRepo GetTournamentRepository()
        {
            return tournamentRepository;
        }

        public List<Team> ScrambleTeamsRandomly(List<Team> teams, bool scramble = true)
        {
            List<Team> scrambledTeams = new List<Team>();
            if (scramble) { 
                int length = teams.Count;
                while (length > 1)
                {
                    int rnd = rng.Next(length);
                    var temp = teams[rnd];
                    teams[rnd] = teams[length - 1];
                    teams[length - 1] = temp;
                    scrambledTeams.Add(teams[length-1]);
                    length--;
                }
            }
            else
            {
                foreach (Team team in teams)
                {
                    scrambledTeams.Add(team);
                }
            }
            return scrambledTeams;
        }
        public void CreateTournament(string name)
        {
            bool takeTeams = true;
            List<Team> teamList = new List<Team>();
            while (takeTeams)
            {
                Console.WriteLine("Indtast holdnavn");
                string TeamName = Console.ReadLine();
                if (TeamName.Length > 0)
                {
                    teamList.Add(new Team(TeamName));
                }
                else
                {
                    takeTeams = false;
                }
            }

            Tournament t = tournamentRepository.GetTournament(name);
            if(t != null)
            {
                throw new Exception("Tournament already exists");
            }
            else
            {
                tournamentRepository.CreateTournament(name, teamList);
            }
        }

        public void RemoveTournament(string name)
        {
            Tournament t = tournamentRepository.GetTournament(name);

            if (t != null)
            {
                tournamentRepository.RemoveTournament(t);
            }
            else
            {
                throw new Exception("Tournament doesn't exist");
            }
        }
    }
}
