using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spielerei_TikTakToe_learning_AI.Data;
using Spielerei_TikTakToe_learning_AI.Model;

namespace Spielerei_TikTakToe_learning_AI.Controller
{
    class Game_PlayerAI
    {
        #region Properties & Konstruktoren
        public Game_Main Game { get; set; }

        public SituationContext DB { get; set; }

        public Game_PlayerAI(Game_Main game, SituationContext context)
        {
            this.Game = game;
            this.DB = context;
        }
        #endregion


        #region zufällige KI
        /// <summary>
        /// Die Fkt. wählt anhand der Spielbrettes, das als Parameter übergeben wurde ein zufälliges, freies Feld (board[i]==0) aus.
        /// Deises wird dann mit der aktuelle Spielernummer beschrieben.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public int[] Random_MakeTurn(int[] board, int player)
        {
            //Erstellen einer Liste aller leeren Felder.
            List<int> possibleMoves = new List<int>();
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0)
                {
                    possibleMoves.Add(i);
                }
            }

            //Bestimmen eines zufälligen Indizes der zuvor erstellten Liste und Setzen des zugeordneten Feldes.
            Random rnd = new Random();
            int moveInformation = rnd.Next(possibleMoves.Count);
            board[possibleMoves[moveInformation]] = player;
            return board;
        }
        #endregion

        #region lernende AI
        /// <summary>
        /// Die lernende AI macht einen Zug, indem es erst die Informationen aus der Datenbank zieht.
        /// Diese Informationen enthalten gewichtete Wahrscheinlichkeiten für jeder freie Feld, wie gut der Zug eventuell ist.
        /// 
        /// Anhand dieser W-Keiten wird ein zufälliger Zug ausgeführt.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public int[] Learning_MakeTurn(int[] board, int player)
        {
            // Variable speichert die Transformation, die nötig ist, 
            //      um vom aktuellen Spielfeld zu seinem Repräsentanten zu gelangen
            int representationNumber = 0;
            // Auslesen der gewichteten Wahrscheinlichkeiten, die in der Datenbank hinterlegt sind und 
            //      überschreiben der obigen Variable.
            int[] possibleMoves = GetWeightedPropabillity(board, out representationNumber);
            // Zufälliges Auswählen eines Zuges nach den zuvor ermittelten Wahrscheinlichkeiten
            //      Der angegebene Zug ist jedoch auf den Repräsentanten des Speilfeldes bezogen.
            int pointToMove = ChooseMoveByWeightedPropability(possibleMoves);
            // Transformieren des zuvor ermittelten Zuges anhand der festgehaltenden Transformation
            pointToMove = TransformMoveFromRepresentation(pointToMove, representationNumber);
            // Ausführen des Zuges
            board[pointToMove] = player;
            return board;
        }

        /// <summary>
        /// Die Funktion nimmt DEN Repräsentanten der aktuellen Spielsituation und gibt 
        ///     die in der Datenbank hinterlegte vorgeschlagende, gewichtete W-Keit an, welche Züge optimal wären.
        /// 
        ///     Sollte die Spielsituation neu sein, wird ein neuer Eintrag in der Datenbank angelegt.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        private int[] GetWeightedPropabillity(int[] board, out int representationNumber)
        {
            int[] possibleMoves = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0};
            board = FindRepresentation(board, out representationNumber);
            string board_String = "";
            string possibleMoves_String = "";
            //Umwandeln von board in einen String, damit der Vergleich mit der DB geschehen kann.
            //Lösung zum folgenden Problem: SQL kann KEINE Arrays Speichern
            for (int i = 0; i < board.Length; i++)
            {
    //alternativ:
                board_String += board[i].ToString() + ",";
                                                            //if (board_String == "")
                                                            //{
                                                            //    board_String += board[i].ToString();
                                                            //}
                                                            //else
                                                            //{
                                                            //    board_String += "," + board[i].ToString();
                                                            //}               
            }
    //alternativ:
            board_String = board_String.Remove(board_String.Length - 1);
            

            //SQL-Abfrage: alle Einträge finden, die die gleiche Spielfeld situation haben
            var query = from situation in DB.Situations
                        where board_String == situation.Board
                        select situation.PossibleMoves;

            //Schauen, ob die Situation in der DB existiert und neues Eintragen dieser in der DB, sollte sie nicht existieren.
            if (query.Count() > 0)
            {
                possibleMoves_String = query.Single();
            }
            else
            {
                Game_Situation Situation_new = newSituation(board);
                DB.Situations.Add(Situation_new);
                DB.SaveChanges();
                possibleMoves_String = Situation_new.PossibleMoves;
            }

            //Umwandeln des Strings in ein Integer-Array, über ein StringArray
            string[] possibleMoves_stringArray = possibleMoves_String.Split(',');
            for (int i = 0; i < possibleMoves_stringArray.Length; i++)
            {
                possibleMoves[i] = Int32.Parse(possibleMoves_stringArray[i]);
            }
            return possibleMoves;
        }

        /// <summary>
        /// Wählt ein Feld aus, quasi zufällig anhand von gewichteten W-Keiten, welche die Funktion "GetWeightedProbabillity" übergeben hat.
        /// </summary>
        /// <param name="possibleMoves"></param>
        /// <returns></returns>
        private int ChooseMoveByWeightedPropability(int[] possibleMoves)
        {
            //Summe aller gewichteten W-keiten durch Aufaddieren der Einzelnen bestimmen
            int total = 0;
            foreach (int item in possibleMoves)
            {
                total += item;
            }

            // W-Keiten quasi Normen, indem die gewichtete WKeit durch die Gesamtsumme geteilt wird.
            // Jeder Tupeleintrag ist die Summe aller vorigen genormten WKeiten, sodass später ein größerKleiner Vergleich mit 
            //      einer Zufälligen Zahl zw. 0..1 geschehen kann.
            double[] possibleMoves_normalized = new double[9];
            for (int i = 0; i < possibleMoves.Length; i++)
            {
                int upperSum = 0;
                for (int j = 0; j <= i; j++)
                {
                    upperSum += possibleMoves[j];
                }
                possibleMoves_normalized[i] = ((double)upperSum / (double)total);
            }

            //Bestimmen einer zufälligen Zahl zw. 0 & 1
            Random rnd = new Random();
            int randomValueInt = rnd.Next(0, 10000);
            double randomValue = (double)randomValueInt / 10000;

            //Bestimmen des Zuges mithilfe der zufälligen Zahl.
            //  Da die Werte in possible_Moves_nomalized in aufsteigender Reihenfolge geordnet sind und
            //      die Abstände von [i-1] zu [i] der Wkeit entsprechen, welche dem Feld [i] zugeordnet wurde, wird das Feld X ausgewählt,
            //      bei dem der "randomValue" im Intervall von possibleMoves_normalized[X-1] bis possibleMoves_normalized[X] liegt.
            //          Anm.: possibleMoves_normalized[-1] = 0
            int move = 0;
            for (int i = 0; i < possibleMoves_normalized.Length; i++)
            {
                if (possibleMoves_normalized[i] > randomValue)
                {
                    move = i;
                    i = 20;
                }
            }
            //Ausgabe des Zuges
            return move;
        }

        /// <summary>
        /// Modifiziert die Einträge in der Datenbank, die die gewichteten Wahrscheinlichkeiten zu den Spielsituationen enthalten.
        /// Es werden nur die Einträge verändert, dessen Speilsituationen von der KI "erlebt " wurden.
        /// Die Modifikation erfolgt in Abhängigkeit dazu, wer das SPiel gewonnen hat. (Variablen change_****)
        ///         Spiel gewonnen:      die gewichtete W-Keit aller Züge der KI werden bei den entsprechenden Spielsituationen um 3 erhöht
        ///         Spiel verloren:      die gewichtete W-Keit aller Züge der KI werden bei den entsprechenden Spielsituationen um 1 verringert
        ///         Spiel unendschieden: die gewichtete W-Keit aller Züge der KI werden bei den entsprechenden Spielsituationen um 3 erhöht
        /// </summary>
        /// <param name="gameHistory"></param>
        /// <param name="decisionHistory"></param>
        /// <param name="winner"></param>
        public void Learn(List<int[]> gameHistory, List<int> decisionHistory, int winner)
        {
            //Lernmodifikatoren
            int change_Win = 3;
            int change_Lose = -1;
            int change_Tie = 1;
            
            //Schleife um jeden Schritt zu evaluieren
            for(int i = 0; i < decisionHistory.Count - 1; i++)
            {
                //Umwandeln der jeweiligen Spielbrettsituation umd damit eine Query in der DB durchzuführen
                //      und den passenden Eintrag zu wählen.
                string board_String = "";
                int representationNumber;
                int[] board_Representation = FindRepresentation(gameHistory[i], out representationNumber);
                board_String += board_Representation[0].ToString();
                for (int j = 1; j < board_Representation.Length; j++)
                {
                    board_String += "," + board_Representation[j].ToString();
                }

                //SQL-Suche
                var query = from situation in DB.Situations
                            where board_String == situation.Board
                            select situation;

                //Umwandeln des getätigten Zuges, damit er zur entsprechednen Repräsentation passt
                int decision = TransformMoveToRepresentation(decisionHistory[i], representationNumber);

                //Lernprozess für den ersten Spieler, der die Züge 1,3,5,7,9 durchführt.
                if ( i == 0 || i == 2 || i == 4 || i == 6 || i == 8 )
                {
                    foreach (var q in query)
                    {
                        string[] possibleMoves_StringArray = q.PossibleMoves.Split(',');
                        //Ermitteln der neuen W-Keit
                        int x = 0;
                        if (winner == -1)
                        {
                            x = change_Win;
                        }
                        else if (winner == 0)
                        {
                            x = change_Tie;
                        }
                        else if (winner == 1)
                        {
                            x = change_Lose;
                        }
                        int probability_new = Int32.Parse(possibleMoves_StringArray[decision]) + x;
        //alternative, falls man nicht möchte, dass Züge ganz ausgeschlossen werden (langsameres Lernen)
                                                    //if (probability_new <= 0)
                                                    //{
                                                    //    probability_new = 1;
                                                    //}

                        //Eintragen der neuen W-Keit in possibleMoves
                        possibleMoves_StringArray[decision] = probability_new.ToString();
                        //Umwandeln von PossibleMoves in den Typ String für die DB
                        string possibleMoves_new = "";
                        foreach (string str in possibleMoves_StringArray)
                        {
                            possibleMoves_new += str + ",";
                        }
                        possibleMoves_new =  possibleMoves_new.Remove(possibleMoves_new.Length - 1);
                        //Eintragen der neuen Wertes in die DB
                        q.PossibleMoves = possibleMoves_new;                     
                    }
                    DB.SaveChanges();
                }
                //Spieler 2 lernen
    //Anmerkung: winner mit aktivem spieler vergleichen sollte diese nervige dopplung umgehen.
                else if (i == 1 || i == 3 || i == 5 || i == 7)
                {
                    if (winner == 1)
                    {
                    }
                    else if (winner == -1)
                    {
                    }
                    else if (winner == 0)
                    {
                    }
                }            
            }
        }
        
        #endregion

        #region Hilfsfunktionen
        /// <summary>
        /// Die Funktion wählt eine zu einer Spielfeldsituation äquivalente Situation aus, 
        ///     die als Repräsentant für alle weiteren äquivalenten Spielfelder steht.
        ///     
        /// Grober Überblick zum Ablauf: Er werden alle äquivalenten Felder erzeugt und für jeden ein spezieller Wert ermittelt.
        ///     Anschliessend wird das Feld mit dem höchsten Wert der Repräsentant. Bei Gleichheiten werden einzelne Felder entfernt,
        ///     sodass nur noch eines zum Ende übrig bleibt.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public int[] FindRepresentation(int[] board, out int RepresentationNumber)
        {
            //alle Möglichen Permutationen, die durch Spielgelung entstehen können werden erzeugt und in eine Liste geschrieben.
            List<int[]> allRepresentations = GetAllRepresentations(board);
            
            RepresentationNumber = 0;
            //Berechnen des internen Repräsentations-Scores
            //  1: ( Positionswert + 1) * 1000                +1, da die Position oben links einen Pos.Wert von Null hat und +0 nichts ändert
            // -1: ( Positionswert + 1)
            //  Summe bilden
            int[] Representation_Score = new int[8] { 0,0,0,0,0,0,0,0};
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (allRepresentations[i][j] == 1)
                    {
                        Representation_Score[i] += (j + 1) * 1000;
                    }
                    else if (allRepresentations[i][j] == -1)
                    {
                        Representation_Score[i] += (j + 1) * 1;
                    }
                }
            }

            //  Bestimmen des maximalen Positionswertes
            //  Im Falle eines Gleichstandes wird der Repräsentant gewählt, dessen Situation eine "1" auf Position 9 (unten rechts) enthält.
            //  Im Falle eines weiteren Gleichstandes wird der Repräsentant gewählt, dessen Situation eine "-1" auf Position 3 (oben rechts) enthält.
            //  Herrscht immernoch Gleichstand, wird der Repräsentant mit dem niedrigsten Index gewählt.
            List<int> RepresentationIndizes = new List<int>();
            for (int i = 0; i < Representation_Score.Length; i++)
            {
                if (Representation_Score.Max() == Representation_Score[i])
                {
                    RepresentationIndizes.Add(i);
                }
            }
            //alle Representationen, die nicht den maximalen Wert haben, werden zu eienr neuen Liste hinzugefügt.
            List<int[]> possibleRepresentations = new List<int[]>();
            for (int i = 7; i >= 0; i--)
            {
                if (RepresentationIndizes.Contains(i))
                {
                    possibleRepresentations.Add(allRepresentations[i]);
                }
            }

            ////Die Gleichstandregel für Position 0/8 implementieren
            //if (possibleRepresentations.Exists(repr => repr[8] == 1))
            //{
            //    if (possibleRepresentations.Exists(repr => repr[0] == 1))
            //    {
            //        foreach (int[] repr in possibleRepresentations)
            //        {
            //            if (possibleRepresentations.Exists(x => x[0] == 1)
            //                    && possibleRepresentations.Exists(x => x[8] != 1))
            //            {
            //                possibleRepresentations.Remove(repr);
                            
            //            }
            //        }
            //    }
            //}
            ////Die Gleichstandregel für Position 3/7 implementieren
            //if (possibleRepresentations.Exists(repr => repr[3] == -1))
            //{
            //    if (possibleRepresentations.Exists(repr => repr[7] == -1))
            //    {
            //        foreach (int[] repr in possibleRepresentations)
            //        {
            //                if (possibleRepresentations.Exists(x => x[7] == -1)
            //                        && possibleRepresentations.Exists(x => x[3] != -1))
            //            {
            //                possibleRepresentations.Remove(repr);
            //            }
            //        }
            //    }
            //}

            //Auswählen des ersten Elementes, da alle übrigbleibenden absolut symmetrisch sind. 
            board = possibleRepresentations.First();

            //Bestimmen des Representanten Indizes.
            for (int i = 0; i < 7; i++)
            {
                if (board == allRepresentations[i])
                {
                    RepresentationNumber = i;
                    i = 100;
                }
                
            }
            return board;
        }

        /// <summary>
        /// Erzeugen eines neuen Objektes der Klasse "Game_Situation", die der DB hinzugefügt wird.
        /// Hierbei wird das aktuelle Spielfeld in eine Eigenschaft übertragen  und
        ///     jedes freie Feld mit gewichteten W-Keit "starting_probability" versehen.
        ///     Besetzte Felder erhalten eine gewichtete W-keit von 0.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        private Game_Situation newSituation(int[] board)
        {
            int starting_Probability = 5;
            string board_String = "";
            string possibleMove_String = "";

            for (int i = 0; i < board.Length; i++)
            {
                //Spielbrett aufnehmen
                if (board_String == "")
                {
                    board_String += board[i].ToString();
                }
                else
                {
                    board_String += "," + board[i].ToString();
                }

                //PossibleMoves bestimmen
                if (board[i] == 0)
                {
                    possibleMove_String += starting_Probability.ToString() + ",";
                }
                else
                {
                    possibleMove_String += "0,";
                }
            }
            possibleMove_String = possibleMove_String.Remove(possibleMove_String.Length - 1);

            Game_Situation situation = new Game_Situation(board_String, possibleMove_String);
            return situation;
        }

        /// <summary>
        /// Wandelt einen in einer Repräsentation ausgeführten Zug in den auf dem Spielfeld äquivalenten Zug um.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="representationNumber"></param>
        /// <returns></returns>
        public int TransformMoveFromRepresentation(int move, int representationNumber )
        {
            int newMove;
            int[] board = new int[9] {0, 1, 2, 3, 4, 5, 6, 7, 8};
            List<int[]> allRepresentations = GetAllRepresentations(board);
            newMove = allRepresentations[representationNumber][move];
            return newMove;
        }

        /// <summary>
        /// Wandelt einen ausgeführten Zug in den zum Repräsentanten äquivalenten Zug um.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="representationNumber"></param>
        /// <returns></returns>
        public int TransformMoveToRepresentation(int move, int representationNumber)
        {
            int newMove = 0;
            int[] board = new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            List<int[]> allRepresentations = GetAllRepresentations(board);
            for (int i = 0; i < 9; i++)
            {
                if (allRepresentations[representationNumber][i] == move)
                {
                    newMove = i;
                }
            }
            return newMove;
        }

        /// <summary>
        /// Gibt eine Liste aller möglichen äquivalenten Stellungen aus.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        private List<int[]> GetAllRepresentations(int[] board)
        {
            List<int[]> representations = new List<int[]>();
            //Identität
            representations.Add(new int[9] { board[0], board[1], board[2], board[3], board[4], board[5], board[6], board[7], board[8] });
            //Drehung -90° (mathematische Drehrichtung)
            representations.Add(new int[9] { board[6], board[3], board[0], board[7], board[4], board[1], board[8], board[5], board[2] });
            //Drehung 180°
            representations.Add(new int[9] { board[8], board[7], board[6], board[5], board[4], board[3], board[2], board[1], board[0] });
            //Drehung 90°
            representations.Add(new int[9] { board[2], board[5], board[8], board[1], board[4], board[7], board[0], board[3], board[6] });
            //Spiegelung an der waagerechten Mitte
            representations.Add(new int[9] { board[6], board[7], board[8], board[3], board[4], board[5], board[0], board[1], board[2] });
            //Spielgelung an  der steigenden Diagonale
            representations.Add(new int[9] { board[8], board[5], board[2], board[7], board[4], board[1], board[6], board[3], board[0] });
            //Spiegelung an der wagerechten Mitte
            representations.Add(new int[9] { board[2], board[1], board[0], board[5], board[4], board[3], board[8], board[7], board[6] });
            //Spiegelung an der fallenden Diagonale
            representations.Add(new int[9] { board[0], board[3], board[6], board[1], board[4], board[7], board[2], board[5], board[8] });

            return representations;
        }
        #endregion
    }
}
