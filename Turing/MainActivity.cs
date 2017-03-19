using System;
using System.Timers;
using System.Collections;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Collections.Generic;
using Android.Views;

namespace Turing {
    [Activity(Label = "Turing", MainLauncher = true, Icon = "@drawable/icon")]

    public class MainActivity : Activity {
        Timer tak;
        int delayTime;
        TextView[] tape;
        List<string> tapeList;
        int index;
        int listIndex;
        state[,] stateTable;
        Hashtable symbolTable;
        LayoutInflater inflater;
        LinearLayout tableSymbols;
        List<LinearLayout> tableRows;
        int rows, cols;
        int row, col;

        struct state {
            public string symbol;
            public string direction;
            public int nextState;
        }
        //SET BUTTONS
        protected void AddState() {
            cols++;
            TextView newText = (TextView)inflater.Inflate(Resource.Layout.StateCellLayout, null);
            newText.SetWidth(tableRows[0].GetChildAt(0).Width);
            newText.SetHeight(tableRows[0].GetChildAt(0).Height);
            newText.Text += cols;

            tableRows[0].AddView(newText, tableRows[0].ChildCount-1);
            
            for(int i = 1; i <= rows; i++) {
                RelativeLayout symbolCell = (RelativeLayout)inflater.Inflate(Resource.Layout.CellLayout, null);
                tableRows[i].AddView(symbolCell);
            }
        }
        protected void AddSymbol() {
            LinearLayout symbolRow = (LinearLayout)inflater.Inflate(Resource.Layout.RowLayout, null);
            tableRows.Add(symbolRow);
            tableSymbols.AddView(symbolRow, tableSymbols.ChildCount - 1);
            for(int i = 0; i <= cols; i++) {
                RelativeLayout symbolCell = (RelativeLayout)inflater.Inflate(Resource.Layout.CellLayout, null);
                symbolRow.AddView(symbolCell);
            }
            rows++;
        }
        protected void SetButtons() {
            Button addState = FindViewById<Button>(Resource.Id.addState);
            addState.Click += delegate {
                AddState();
            };
            Button addSymbol = FindViewById<Button>(Resource.Id.addSymbol);
            addSymbol.Click += delegate {
                AddSymbol();
            };
        }

        protected void TmpStates() {
            stateTable = new state[3, 7];
            //#
            stateTable[0, 0].symbol = "#"; stateTable[0, 0].direction = "R"; stateTable[0, 0].nextState = 0;
            stateTable[0, 1].symbol = "#"; stateTable[0, 1].direction = "R"; stateTable[0, 1].nextState = 2;
            stateTable[0, 2].symbol = "0"; stateTable[0, 2].direction = "L"; stateTable[0, 2].nextState = 3;
            stateTable[0, 3].symbol = "#"; stateTable[0, 3].direction = "L"; stateTable[0, 3].nextState = 4;
            stateTable[0, 4].symbol = "#"; stateTable[0, 4].direction = "R"; stateTable[0, 4].nextState = 0;
            stateTable[0, 5].symbol = "#"; stateTable[0, 5].direction = "R"; stateTable[0, 5].nextState = 6;
            stateTable[0, 6].symbol = "1"; stateTable[0, 6].direction = "L"; stateTable[0, 6].nextState = 3;
            //0
            stateTable[1, 0].symbol = "#"; stateTable[1, 0].direction = "R"; stateTable[1, 0].nextState = 1;
            stateTable[1, 1].symbol = "0"; stateTable[1, 1].direction = "R"; stateTable[1, 1].nextState = 1;
            stateTable[1, 2].symbol = "0"; stateTable[1, 2].direction = "R"; stateTable[1, 2].nextState = 2;
            stateTable[1, 3].symbol = "0"; stateTable[1, 3].direction = "L"; stateTable[1, 3].nextState = 3;
            stateTable[1, 4].symbol = "0"; stateTable[1, 4].direction = "L"; stateTable[1, 4].nextState = 4;
            stateTable[1, 5].symbol = "0"; stateTable[1, 5].direction = "R"; stateTable[1, 5].nextState = 5;
            stateTable[1, 6].symbol = "0"; stateTable[1, 6].direction = "R"; stateTable[1, 6].nextState = 6;
            //1
            stateTable[2, 0].symbol = "#"; stateTable[2, 0].direction = "R"; stateTable[2, 0].nextState = 5;
            stateTable[2, 1].symbol = "1"; stateTable[2, 1].direction = "R"; stateTable[2, 1].nextState = 1;
            stateTable[2, 2].symbol = "1"; stateTable[2, 2].direction = "R"; stateTable[2, 2].nextState = 2;
            stateTable[2, 3].symbol = "1"; stateTable[2, 3].direction = "L"; stateTable[2, 3].nextState = 3;
            stateTable[2, 4].symbol = "1"; stateTable[2, 4].direction = "L"; stateTable[2, 4].nextState = 4;
            stateTable[2, 5].symbol = "1"; stateTable[2, 5].direction = "R"; stateTable[2, 5].nextState = 5;
            stateTable[2, 6].symbol = "1"; stateTable[2, 6].direction = "R"; stateTable[2, 6].nextState = 6;

            symbolTable = new Hashtable();
            symbolTable.Add("#", 0);
            symbolTable.Add("0", 1);
            symbolTable.Add("1", 2);
        }
        //Turing Machine
        protected void EndWork() {
            tak.Stop();
        }
        protected void RunHead(string direction) {
            if (direction.Equals("R")) index++;
            else if (direction.Equals("L")) index--;
            else EndWork();
        }
        protected void TuringMachine() {
            int i = 0;
            for(int j = index; j < 11+index; i++) {
                if (j < 0 || j >= tapeList.Count) tape[i].Text = "#";
                else tape[i].Text = tapeList[j];
                j++;
            }
            row = (int)symbolTable[tape[5].Text];
            string symbol = stateTable[row, col].symbol;
            string direction = stateTable[row, col].direction;
            int state = stateTable[row, col].nextState;
            if (index + 5 < 0) {
                tapeList.Insert(0, symbol);
                index++;
                listIndex = 0;
            }
            else if (index + 5 >= tapeList.Count) {
                tapeList.Add(symbol);
                listIndex = tapeList.Count - 1;
            }
            if (index + 5 >= 0 && index + 5 <= tapeList.Count - 1) {
                tapeList[index + 5] = symbol;
                listIndex = index + 5;
            }
            Handler h = new Handler();
            Action myAction = () => {
                tape[5].Text = tapeList[listIndex];
            };
            h.PostDelayed(myAction, delayTime/2);
            if(direction != null)
                RunHead(direction);
            col = state;
        }

        protected void Timer_Repeat(object sender, ElapsedEventArgs e) {
            RunOnUiThread (delegate {
                TuringMachine();
            });
        }

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            tak = new Timer();
            SetContentView (Resource.Layout.Main);

            TmpStates();
            cols = 0;
            SetButtons();

            tape = new TextView[11];
            tapeList = new List<string>();
            for(int i = 0; i < tape.Length; i++) {
                tape[i] = FindViewById<TextView>(Resource.Id.tasma0 + i);
                tape[i].Text = "#";
            }
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            index = -5;
            col = 0;

            delayTime = 1000;
            tak.Interval = delayTime;
            tak.Elapsed += new ElapsedEventHandler(Timer_Repeat);
            tak.Start();
            
            inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);

            tableRows = new List<LinearLayout>();
            LinearLayout stateRow = FindViewById<LinearLayout>(Resource.Id.stateRow);
            tableRows.Add(stateRow);

            tableSymbols = FindViewById<LinearLayout>(Resource.Id.stateTabel);
            LinearLayout blankSymbolRow = (LinearLayout)inflater.Inflate(Resource.Layout.RowLayout, null);
            RelativeLayout blankSymbolCell = (RelativeLayout)inflater.Inflate(Resource.Layout.CellLayout, null);
            tableRows.Add(blankSymbolRow);
            tableSymbols.AddView(blankSymbolRow, stateRow.ChildCount-2);
            blankSymbolRow.AddView(blankSymbolCell);
            rows = 1;

        }
    }
}

