using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class RPSGame
{
    public Guid p1, p2;
    public RPS p1c, p2c;

    public RPSGame(Guid p1, Guid p2)
    {
        this.p1 = p1;
        this.p2 = p2;
        p1c = RPS.NONE;
        p2c = RPS.NONE;
    }

    public bool Matches(Guid a, Guid b)
    {
        return (a == p1 && b == p2) || (a == p2 && b == p1);
    }

    public void SetChoice(Guid p, string choice)
    {
        switch (choice.ToLower())
        {
            case "rock":
                if (p == p1)
                    p1c = RPS.ROCK;
                else if (p == p2)
                    p2c = RPS.ROCK;
                break;
            case "paper":
                if (p == p1)
                    p1c = RPS.PAPER;
                else if (p == p2)
                    p2c = RPS.PAPER;
                break;
            case "scissors":
                if (p == p1)
                    p1c = RPS.SCISSORS;
                else if (p == p2)
                    p2c = RPS.SCISSORS;
                break;
            default:
                break;
        }
    }

    public RPS GetChoice(Guid p)
    {
        if (p == p1)
            return p1c;
        else if (p == p2)
            return p2c;
        return RPS.NONE;
    }

    public bool IsReady()
    {
        return p1c != RPS.NONE && p2c != RPS.NONE;
    }

    public Guid GetWinner()
    {
        if (p1c == RPS.ROCK)
        {
            if (p2c == RPS.ROCK)
                return Guid.Empty;
            else if (p2c == RPS.PAPER)
                return p2;
            else if (p2c == RPS.SCISSORS)
                return p1;
        }
        else if (p1c == RPS.PAPER)
        {
            if (p2c == RPS.ROCK)
                return p1;
            else if (p2c == RPS.PAPER)
                return Guid.Empty;
            else if (p2c == RPS.SCISSORS)
                return p2;
        }
        else if (p1c == RPS.SCISSORS)
        {
            if (p2c == RPS.ROCK)
                return p2;
            else if (p2c == RPS.PAPER)
                return p1;
            else if (p2c == RPS.SCISSORS)
                return Guid.Empty;
        }
        return Guid.Empty;
    }

    public enum RPS
    {
        NONE, ROCK, PAPER, SCISSORS
    }
}
