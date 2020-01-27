using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Spielerei_TikTakToe_learning_AI.Controller;
using Spielerei_TikTakToe_learning_AI.Hilfsklasse;
using Spielerei_TikTakToe_learning_AI.Model;

namespace Spielerei_TikTakToe_learning_AI.ViewModel
{
    class VM_MainWindow : INotifyPropertyChanged
    {
        #region Props & Konstruktoren
        //Allgemeine Properties, die hier wohl nicht hingehören
        private Boolean GameInProgress { get; set; }
        private Game_Main Game { get; set; }
        public Game_State GameState { get; set; }

        //Command-Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand Cmd_FieldInput { get; set; }
        public Boolean CanExecute_Field { get; set; }
        public ICommand ToggleExecute_Field { get; set; }

        public ICommand Cmd_Mode { get; set; }
        public Boolean CanExecute_Menu { get; set; }
        public ICommand ToggleExecute_Menu { get; set; }


        //Anzeige-Properties
        public int[] FieldContent { get; set; }
        public string TextContent { get; set; }

        //Konstruktor
        public VM_MainWindow()
        {
            GameInProgress = false;
            GameState = new Game_State();

            CanExecute_Field = false;
            CanExecute_Menu = true;
            Cmd_FieldInput = new RelayCommand(FieldInput, parameter => CanExecute_Field);
            Cmd_Mode = new RelayCommand(ChooseMode, parameter => CanExecute_Menu);
            Cmd_Test = new RelayCommand(Fkt_Test);
            FieldContent = new int[9];
            TextContent = "Allgemeine Dinge";
        }
        #endregion


        #region Hintergrundkram
        public bool CanExecute { get; set; }

        protected void OnNotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region PlayArea
        /// <summary>
        /// Diese Funktion wird aufgerufen, wenn der Spieler ein Feld anklickt. 
        /// Hierbei wird die Koordinate des Feldes als Parameter übergeben und die Funktion 
        /// MakeTurn des Objektes Game gestartet, bei dem wieder die Feldkoordinate übergeben wird.
        /// </summary>
        /// <param name="obj"></param>
        private void FieldInput(object obj)
        {
            if (GameInProgress)
            {
                string point_string = obj.ToString();
                int point = Int32.Parse(point_string);
                Game.MakeTurn(point);               
            }
        }

        /// <summary>
        /// Diese Fuktion wird nach jedem Spielzug von dem Objekt "Game" aufgerufen.
        /// Ist das Spiel beendet (Informationen darübe wird im Object "GameState" gespeichert), so werden 
        ///     die Felder des Spielfeldes deaktiviert,
        ///     die Felder zum Start eines neuen Spiels aktiviert und
        ///     eine entsprechende Nachricht angezeigt.
        /// Sollte das Spiel nicht beendet sein, wird entweder die Nachricht über den aktiven Spieler angezeigt 
        ///     oder eine Meldung ausgegeben, dass der Zug ungültig war.
        ///  
        /// </summary>
        public void Display_EndOfTurn()
        {
            switch (GameState.GameOver)
            {
                //Spiel noch nicht beendet
                case false:
                    if (GameState.ActivePlayer == -1)
                        TextContent = "Spieler O ist am Zug.";
                    else if (GameState.ActivePlayer == 1)
                        TextContent = "Spieler X ist am Zug.";
                    break;
                //Spiel ist beendet
                case true:
                    if (GameState.Winner == 1)
                        TextContent = "Spieler X hat gewonnen.";
                    else if (GameState.Winner == -1)
                        TextContent = "Spieler O hat gewonnen.";
                    else if (GameState.Winner == 0)
                        TextContent = "Heute gewinnt der Spaß.";
                    CanExecute_Field = false;
                    CanExecute_Menu = true;
                    GameInProgress = false;
                    break;

                default:
                    TextContent = TextContent + "\r\nZug ungültig";
                    break;
            }
            OnNotifyPropertyChanged("GameState");
            OnNotifyPropertyChanged("TextContent");
        }
        #endregion


        #region Modi
        /// <summary>
        /// Die Funktion wird aufgerufen, wenn ein Feld der verschiedenen Modi angeklickt wird.
        /// Hierbei wird ein Parameter übergeben, der den gewählten Modus beschreibt.
        /// Im Anschluss wird eine neues Objekt der Klasse Game_Main erzeugt und damit ein Spielgestartet, 
        ///     indem die Funktion Game.Start aufgerufen wird und als Parameter die Informationen über den gewählten Modus übergeben.
        ///         1.Stelle: Startspieler      2.Stelle: zweiter Spieler
        ///         Einträge: 0:Nutzer, 1:zufällige KI, 2:lernende KI
        /// Auch werden im Anschluss die Button für die Modi deaktiviert und die Feldbuttons aktiviert.
        /// </summary>
        /// <param name="obj"></param>
        public void ChooseMode(object obj)
        {
            CanExecute_Field = true;
            CanExecute_Menu = false;
            GameInProgress = true;
            Game = new Game_Main(this);

            int[] playerTypes = new int[2];
            string param = obj.ToString();
            for (int i = 0; i < 2; i++)
            {
                playerTypes[i] = Int32.Parse(param.Substring(i, 1));
            }
            Game.Start(playerTypes);
            OnNotifyPropertyChanged("GameState");
        }


        public void AiVsRandom_100times(object obj)
        {

        }
        #endregion


        #region Testkram
        public ICommand Cmd_Test { get; set; }
        public void Fkt_Test(object obj)
        {
            //if (obj is object[])
            //{
            //    var param = obj as object[];
            //    FieldContent[4] = param[0].ToString();
            //    OnNotifyPropertyChanged("FieldContent");
            //    TextContent = param[1].ToString();
            //    OnNotifyPropertyChanged("TextContent");
            //}
            //else
            //{
            //    string input = obj as String;
            //    char[] seperator = { '|' };
            //    string[] param = input.Split(seperator);
            //    FieldContent[4] = param[0].ToString();
            //    OnNotifyPropertyChanged("FieldContent");
            //    TextContent = param[1].ToString();
            //    OnNotifyPropertyChanged("TextContent");
            //}

            TextContent = CanExecute.ToString() + " " + GameInProgress.ToString();
            OnNotifyPropertyChanged("TextContent");
            OnNotifyPropertyChanged("FieldContent");
        }
        #endregion
    }
}
