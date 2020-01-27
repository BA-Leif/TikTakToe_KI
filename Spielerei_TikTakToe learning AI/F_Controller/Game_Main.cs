using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spielerei_TikTakToe_learning_AI.ViewModel;
using Spielerei_TikTakToe_learning_AI.Model;
using Spielerei_TikTakToe_learning_AI.Data;

namespace Spielerei_TikTakToe_learning_AI.Controller
{
    class Game_Main
    {
        #region Properties & Konstruktor
        public Game_Main(VM_MainWindow view)
        {
            this.VM = view;
            GameState = new Game_State();

            GameHistory = new List<int[]>();
            DecisionHistory = new List<int>();

            AIInformation = new int[2];
            Context = new SituationContext();
            AI = new Game_PlayerAI(this, Context);
        }

        //Spielsituation
                //GameState.Board (Spielfeld)
                //Enthält das 3x3 Spielfeld als 9er Array.
                // Schlüssel:   0.Stelle    1.Stelle    2.Stelle
                //              3.Stelle    4.Stelle    5.Stelle
                //              6.Stelle    7.Stelle    8.Stelle
                // Inhalt:
                // 0: leeres Feld
                //+1: Feld des ersten Spielers (X)
                //-1: Feld des zweiten Spielers (O)
        public Game_State GameState { get; set; }

        //Referenz zum ViewModel
        public VM_MainWindow VM { get; set; }

        //KI-Spieler 
            //0: Spieler
            //1: Zufällige KI
            //2: lernende KI
        private int[] AIInformation { get; set; }
        private Game_PlayerAI AI { get; set; }
        //Datenbank, die die Informationen für die lernende KI bereitstellt und speichert
        private SituationContext Context { get; set; }

        //Enthält alle Stellugnen des Spiels in chronologischer Reihenfolge
        private List<int[]> GameHistory { get; set; }
        //Enthält alle Entscheidungen des Spiel in chronologischer Reihenfolge
        private List<int> DecisionHistory { get; set; }
        #endregion


        #region Modi
        /// <summary>
        /// Initialisiert Eigenschaften, die abhängig vom Modus sind und lässt die KI den ersten Zug ausführen.
        /// </summary>
        /// <param name="playerTypes"></param>
        public void Start(int[] playerTypes)
        {
            AIInformation = playerTypes;
            ChooseAI();
            SendInformationToVM();
        }
        #endregion


        #region Spielmechaniken
        /// <summary>
        /// Testet, ob das angegebene Feld frei war. Sollte es nicht frei gewesen sein, so passiert nichts.
        /// Ist es ein Freies Feld, so wird es mit der Nummer des Spieler überschrieben,
        ///     
        /// 
        /// Die AusgabeVariable gibt dem ViewModel die Informationen, ob der Zug gültig war und welcher Spieler diesen durchgeführt hat.
        ///     Ausgabe = 0 : Zug ungültig
        ///               1 : Spieler "X" hat gezogen
        ///               5 : Spieler "X" hat gewonnen
        ///              -1 : Spieler "O" hat gezogen
        ///              -5 : Spieler "O" hat gewonnen
        ///             100 : Spiel mit Unendschieden beendet
        ///             
        /// Speichert die Ausgangslage (GameHistory) und die erfolgte Entscheidung(DecisionHistory) in den entsprechenden Variablen.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public void MakeTurn(int point)
        {
            if (GameState.Board[point] == 0)
            {
                GameHistory.Add(GameState.Board.Clone() as int[]);
                GameState.Board[point] = GameState.ActivePlayer;
                DecisionHistory.Add(point);
                TestForEndCondition();
                PlayerChange();
                ChooseAI();
                SendInformationToVM();
            }

        }

        /// <summary>
        /// Überprüft mithilfe von Summenbildung alle Siegbedingungen des Spiels.
        /// Tritt eine Siegbedingung oder ein Spielende ein, so wird der Sieger in GameState gesetzt 
        ///     und die Variable "GameOver" auf TRUE gesetzt.
        /// Im Anschluss werden die Informationen über den Spielverlauf an das KI-Objekt weitergegeben,
        ///     die dann mithilfe derer die Datenbankeinträge anpasst und "lernt"
        /// </summary>
        /// <returns></returns>
        private void TestForEndCondition()
        {
            //zuerst das Spielende nach 9 Zügen aufnehmen, wenn also keine freien Felder mehr da sind
            if (!GameState.Board.Contains(0))
            {
                GameState.GameOver = true;
                GameState.Winner = 0;
            }

            //im Anschluss, falls jmd. mit dem letzten Zug gewinnt, werden unter Umständen die Variablen ein zweites Mal überschrieben.
            if ((Math.Abs(GameState.Board[0] + GameState.Board[1] + GameState.Board[2]) == 3) ||
                (Math.Abs(GameState.Board[3] + GameState.Board[4] + GameState.Board[5]) == 3) ||
                (Math.Abs(GameState.Board[6] + GameState.Board[7] + GameState.Board[8]) == 3) ||
                (Math.Abs(GameState.Board[0] + GameState.Board[3] + GameState.Board[6]) == 3) ||
                (Math.Abs(GameState.Board[1] + GameState.Board[4] + GameState.Board[7]) == 3) ||
                (Math.Abs(GameState.Board[2] + GameState.Board[5] + GameState.Board[8]) == 3) ||
                (Math.Abs(GameState.Board[0] + GameState.Board[4] + GameState.Board[8]) == 3) ||
                (Math.Abs(GameState.Board[2] + GameState.Board[4] + GameState.Board[6]) == 3)   )
            {
                GameState.GameOver = true;
                GameState.Winner = GameState.ActivePlayer;               
            }
            if (GameState.GameOver)
            {
                if (AIInformation[0] != 0)
                    AI.Learn(GameHistory, DecisionHistory, GameState.Winner);
            }
        }
        #endregion


        #region KI Funktionen
        /// <summary>
        /// Die Funktion startet aufgrund der Tatsache, ob der erste und eventuell zweite Spieler eine KI ist, 
        ///     die Funktion MakeTurn_AI auf.
        /// Ist auch der zweite Spiler eine KI, so ruft die Fkt. sich selbst auf, bis das Spiel zuende ist.
        /// </summary>
        private void ChooseAI()
        {
            if (!GameState.GameOver)
            {
                //erster Spieler eine KI?
                if (AIInformation[0] != 0)
                {
                    MakeTurn_AI(AIInformation[0] == 2);
                }
                //zweiter Spieler eine KI?
                if (!GameState.GameOver)
                {
                    if (AIInformation[1] != 0)
                    {
                        MakeTurn_AI(AIInformation[1] == 2);
                    }
                }
                //Selbsterhaltende Scheife, wenn der zweite Spieler eine KI ist, da in diesem Fall BEIDE Spieler eine KI sein müssen.
                //  (Wenn eine KI im Spiel ist, ist Immer der erste Spieler eine KI)
                if (AIInformation[1] != 0 && !GameState.GameOver)
                    ChooseAI();
            }
        }

        /// <summary>
        /// Startet - abhängig davon, ob die KI zufällig oder lernend ist- die Funktion MakeTurn des Objektes AI.
        /// Im Anschluss werden die Funktionen für das Spielende, Spielerwechsel und die Aktualisierung der View aufgerufen.
        /// 
        /// Speichert die Ausgangslage (GameHistory) und die erfolgte Entscheidung(DecisionHistory) in den entsprechenden Variablen.
        /// </summary>
        /// <param name="learning_AI"> Ist die KI eine lernende KI?</param>
        private void MakeTurn_AI(Boolean learning_AI)
        {
            //aktuelle Spielsituation wird gespeichert
            GameHistory.Add(GameState.Board.Clone() as int[]);

            if (!learning_AI)
            {
                GameState.Board = AI.Random_MakeTurn(GameState.Board, GameState.ActivePlayer);
            }
            else if (learning_AI)
            {
                GameState.Board = AI.Learning_MakeTurn(GameState.Board, GameState.ActivePlayer);
            }

            //Entscheidung der KI wird gespeichert, nachdem der letzte Zug der KI ermittelt wurde.
            for (int i = 0; i < 9; i++)
            {
                if (GameState.Board[i] != GameHistory.Last()[i]
                    && GameState.Board[i] == GameState.ActivePlayer)
                {
                    DecisionHistory.Add(i);
                }
            }
            TestForEndCondition();
            PlayerChange();
            SendInformationToVM();
        }
        #endregion


        #region Hilfsfunktionen
        //Überschreiben der GameState-Eigenschaft im ViewModel durch den Wert der aktuellen GameState-Eigenschaft DIESER Klasse
        //      und anschliessendes Starten einer Funktion im VM, die die anzeige aktualisiert
        private void SendInformationToVM()
        {
            VM.GameState = GameState;
            VM.Display_EndOfTurn();
        }

        //Wechselt den aktiven Spieler. Punkt.
        private void PlayerChange()
        {
            if (GameState.ActivePlayer == 1)
                GameState.ActivePlayer = -1;
            else
                GameState.ActivePlayer = 1;
        }
        #endregion
    }
}
