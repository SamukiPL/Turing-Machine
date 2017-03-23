using System;
using System.Timers;
using System.Collections;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Collections.Generic;
using Android.Views;

namespace Turing {
    [Activity (Label = "Turing Machine", MainLauncher = true, Icon = "@drawable/icon",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation, 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]

    public class MainActivity : Activity {
        Timer machineTimer;
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
        Drawable editTextDrawable;

        struct state {
            public string symbol;
            public string direction;
            public int nextState;
        }
        //ADDING THINGS TO TABLE
        protected void ChangeState(RelativeLayout layout) {
            layout.Click += delegate {
                layout.SetBackgroundColor(Color.Argb(1, 62,62,62));

                View dialogLayout = inflater.Inflate(Resource.Layout.CellAlertLayout, null);
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);

                EditText newDirection = dialogLayout.FindViewById<EditText>(Resource.Id.newDirection);
                EditText newSymbol = dialogLayout.FindViewById<EditText>(Resource.Id.newSymbol);
                EditText newState = dialogLayout.FindViewById<EditText>(Resource.Id.newState);

                TextView direction = layout.FindViewById<TextView>(Resource.Id.direction);
                TextView symbol = layout.FindViewById<TextView>(Resource.Id.symbol);
                TextView state = layout.FindViewById<TextView>(Resource.Id.state);

                newDirection.Text = direction.Text;
                newSymbol.Text = symbol.Text;
                newState.Text = state.Text;

                dialog.SetView(dialogLayout);
                dialog.SetTitle("Insert data");
                dialog.SetCancelable(false)
                    .SetPositiveButton("OK", (a, b) => {
                        direction.Text = newDirection.Text;
                        symbol.Text = newSymbol.Text;
                        state.Text = newState.Text;
                        SetImmersiveMode();
                    })
                    .SetNegativeButton("Cancel", (a, b) => {
                        SetImmersiveMode();
                    });
                dialog.Show();
            };
        }
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
                ChangeState(symbolCell);
            }
        }

        protected void ChangeSymbol(TextView view) {
            view.Click += delegate {
                View dialogLayout = inflater.Inflate(Resource.Layout.SymbolAlertLayout, null);
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);

                EditText newSymbol = dialogLayout.FindViewById<EditText>(Resource.Id.newSymbol);
                newSymbol.Text = view.Text;

                dialog.SetView(dialogLayout);
                dialog.SetTitle(" ");
                dialog.SetCancelable(false)
                    .SetPositiveButton("OK", (a, b) => {
                        view.Text = Convert.ToString(newSymbol.Text[0]);
                        SetImmersiveMode();
                    })
                    .SetNegativeButton("Cancel", (a, b) => {
                        SetImmersiveMode();
                    });
                dialog.Show();
            };
        }
        protected void AddSymbol() {
            LinearLayout symbolRow = (LinearLayout)inflater.Inflate(Resource.Layout.RowLayout, null);
            tableRows.Add(symbolRow);

            RelativeLayout firstLayout = (RelativeLayout)symbolRow.GetChildAt(0);
            ChangeSymbol((TextView)firstLayout.GetChildAt(1));

            tableSymbols.AddView(symbolRow, tableSymbols.ChildCount - 1);
            for(int i = 0; i < cols; i++) {
                RelativeLayout symbolCell = (RelativeLayout)inflater.Inflate(Resource.Layout.CellLayout, null);
                ChangeState(symbolCell);
                symbolRow.AddView(symbolCell);
            }
            rows++;
        }

        //SET ON THE START
        protected void SetImmersiveMode() {
            View decorView = Window.DecorView;
            var uiOption = (int)decorView.SystemUiVisibility;

            uiOption |= (int)SystemUiFlags.Fullscreen;
            uiOption |= (int)SystemUiFlags.Immersive;
            uiOption |= (int)SystemUiFlags.HideNavigation;
            uiOption |= (int)SystemUiFlags.LayoutFullscreen;

            decorView.SystemUiVisibility = (StatusBarVisibility)uiOption;
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
            Button startMachine = FindViewById<Button>(Resource.Id.startMachine);
            startMachine.Click += delegate {
                EndWork();
                StartWork();
            };
            EditText inputView = FindViewById<EditText>(Resource.Id.input);
            inputView.TextChanged += delegate {
                inputView.SetBackgroundDrawable(editTextDrawable);
            };
        }
        protected void SetTape() {
            cols = 1;
            rows = 1;

            tape = new TextView[11];
            tapeList = new List<string>();
            for (int i = 0; i < tape.Length; i++) {
                tape[i] = FindViewById<TextView>(Resource.Id.tape0 + i);
                tape[i].Text = "#";
            }
            index = -5;
            col = 0;
        }
        protected void SetBlankSymbol() {
            LinearLayout stateRow = FindViewById<LinearLayout>(Resource.Id.stateRow);
            tableRows.Add(stateRow);

            LinearLayout blankSymbolRow = (LinearLayout)inflater.Inflate(Resource.Layout.RowLayout, null);
            RelativeLayout blankSymbolCell = (RelativeLayout)inflater.Inflate(Resource.Layout.CellLayout, null);
            tableRows.Add(blankSymbolRow);
            tableSymbols.AddView(blankSymbolRow, stateRow.ChildCount - 2);
            blankSymbolRow.AddView(blankSymbolCell);
            ChangeState(blankSymbolCell);
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

            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
            tapeList.Add("0");
            tapeList.Add("1");
        }

        //TURING MACHINE
        protected void StartWork() {
            bool readyToStart = false;
            //SET SYMBOLS
            symbolTable.Clear();
            for (int i = 0; i < rows; i++) 
                symbolTable.Add(tableRows[i + 1].GetChildAt(0).FindViewById<TextView>(Resource.Id.currentSymbol).Text, i);
            //SET STATE TABLE
            stateTable = new state[rows, cols];
            for(int i = 0; i < rows; i++) {
                for(int j = 0; j < cols; j++) {
                    try {
                        stateTable[i, j].direction = tableRows[i + 1].GetChildAt(j + 1).FindViewById<TextView>(Resource.Id.direction).Text;
                        stateTable[i, j].symbol = tableRows[i + 1].GetChildAt(j + 1).FindViewById<TextView>(Resource.Id.symbol).Text;
                        stateTable[i, j].nextState = Convert.ToInt32(
                            tableRows[i + 1].GetChildAt(j + 1).FindViewById<TextView>(Resource.Id.state).Text.Substring(1));
                    }
                    catch (Exception e) {
                        tableRows[i + 1].GetChildAt(j + 1).SetBackgroundColor(Color.Red);
                        break;
                    }
                }
            }
            //SET INPUT
            tapeList.Clear();
            EditText inputView = FindViewById<EditText>(Resource.Id.input);
            string input = inputView.Text;
            if (input.Length != 0) {
                try {
                    for (int i = 0; i < input.Length; i++) {
                        int tmp = (int)symbolTable[Convert.ToString(input[i])];
                        tapeList.Add(Convert.ToString(input[i]));
                    }
                    readyToStart = true;
                }
                catch (Exception e) {
                    readyToStart = false;
                    inputView.SetBackgroundColor(Color.Red);
                }
            }
            else
                tapeList.Add("#");
            //SET DELAY TIME
            EditText delayView = FindViewById<EditText>(Resource.Id.delay);
            try {
                int tmpDelay = Convert.ToInt32("" + delayView.Text) * 1000;
                if (tmpDelay > 1)
                    delayTime = tmpDelay;
                else
                    delayTime = 500;
            }
            catch (Exception e) {
                delayTime = 500;
            }
            if (readyToStart) {
                machineTimer.Interval = delayTime;
                machineTimer.Start();
            }
        }
        protected void EndWork() {
            machineTimer.Stop();
        }
        protected void RunHead(string direction) {
            if (direction.Equals("R")) index++;
            else if (direction.Equals("L")) index--;
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
            h.PostDelayed(myAction, delayTime / 2);
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
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetImmersiveMode();
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
            tableRows = new List<LinearLayout>();
            tableSymbols = FindViewById<LinearLayout>(Resource.Id.stateTabel);

            SetButtons();
            SetTape();
            SetBlankSymbol();
            TmpStates();
            machineTimer = new Timer();
            delayTime = 1000;
            machineTimer.Elapsed += new ElapsedEventHandler(Timer_Repeat);

            editTextDrawable = FindViewById<EditText>(Resource.Id.input).Background;
        }

        public override void OnUserInteraction() {
            base.OnUserInteraction();
            SetImmersiveMode();
        }
    }
}

