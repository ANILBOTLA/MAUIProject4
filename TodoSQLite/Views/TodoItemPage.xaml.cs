using System.Linq.Expressions;
using TodoSQLite.Data;
using TodoSQLite.Models;

namespace TodoSQLite.Views;

[QueryProperty("Item", "Item")]
public partial class TodoItemPage : ContentPage
{
	TodoItem item;
    List<string> Expressions = new List<string>();
    Stack<double> values = new Stack<double>();
    Stack<Char> ops = new Stack<char>();


    string currentEntry = "";
    int currentState = 1;
    string mathOperator;
    double firstNumber, secondNumber;
    string decimalFormat = "N0";
    bool expression = false;
    string eval = "";

    public TodoItem Item
	{
		get => BindingContext as TodoItem;
		set => BindingContext = value;
	}
    TodoItemDatabase database;
    public TodoItemPage(TodoItemDatabase todoItemDatabase)
    {
        InitializeComponent();
        database = todoItemDatabase;
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Item.Name))
        {
            await DisplayAlert("Name Required", "Please enter a name for the todo item.", "OK");
            return;
        }

        await database.SaveItemAsync(Item);
        await Shell.Current.GoToAsync("..");
    }

  



    async void OnDeleteClicked(object sender, EventArgs e)
    {
        Expressions.Add(CurrentCalculation.Text + " = " + resultText.Text);
        for (int i = 0; i < Expressions.Count; i++)
        {
            Item.Name = "Deleted";
        }
        object value = await database.UpdateAsync(Item);
       
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        firstNumber = 0;
      secondNumber = 0;
      currentState = 1;
      decimalFormat = "N0";
      this.resultText.Text = "0";
      this.currentEntry = string.Empty;
      this.eval = "";
      this.expression = false;
      this.CurrentCalculation.Text = "";
      
    }

    private void OnNegative(System.Object sender, EventArgs e)
    {
        if (currentState == 1)
        {
            secondNumber = -1;
            mathOperator = "×";
            currentState = 2;
            OnCalculate(this, null);
        }
    }


    public void OnSelectNumber(System.Object sender, EventArgs e)
    {

        Button button = (Button)sender;
        string pressed = button.Text;

        currentEntry += pressed;
        this.eval += pressed;

        if ((this.resultText.Text == "0" && pressed == "0")
            || (this.currentEntry.Length <= 1 && pressed != "0")
            || currentState < 0)
        {
            this.resultText.Text = "";
            if (currentState < 0)
                currentState *= -1;
        }

        if (pressed == "." && decimalFormat != "N2")
        {
            decimalFormat = "N2";
        }

        if (this.expression)
        {
            this.resultText.Text += this.eval;
        }
        else
        {
            this.resultText.Text += pressed;
        }
        this.values.Push(int.Parse(pressed));
    }

    public void OnSelectOperator(System.Object sender, EventArgs e)
    {
        LockNumberValue(resultText.Text);

        currentState = -2;
        Button button = (Button)sender;
        string pressed = button.Text;
        mathOperator = pressed;

        this.eval += pressed;
        if (pressed == "(")
        {
            this.ops.Push(char.Parse(pressed));
            expression = true;
        }
        else if (pressed == ")")
        {
            while (this.ops.Peek() != '(')
            {
                this.values.Push(Calculator.Calculate(this.values.Pop(), this.values.Pop(), (this.ops.Pop()).ToString()));
            }
            this.ops.Pop();
        }
        else if (pressed == "+" || pressed == "-" || pressed == "×" || pressed == "/")
        {

            while (this.ops.Count > 0 && hasPrecedence(char.Parse(pressed), this.ops.Peek()))
            {
                this.values.Push(Calculator.Calculate(this.values.Pop(), this.values.Pop(), (this.ops.Pop()).ToString()));
            }
            this.ops.Push(char.Parse(pressed));
        }

        this.resultText.Text = this.eval;


    }

    public static bool hasPrecedence(char op1, char op2)
    {
        if (op2 == '(' || op2 == ')')
        {
            return false;
        }
        if ((op1 == '*' || op1 == '/') &&
            (op2 == '+' || op2 == '-'))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void LockNumberValue(string text)
    {
        double number;
        if (double.TryParse(text, out number))
        {
            if (currentState == 1)
            {
                firstNumber = number;
            }
            else
            {
                secondNumber = number;
            }

            this.currentEntry = string.Empty;
        }
    }

    public void OnClear(System.Object sender, EventArgs e)
    {
        firstNumber = 0;
        secondNumber = 0;
        currentState = 1;
        decimalFormat = "N0";
        this.resultText.Text = "0";
        this.currentEntry = string.Empty;
        this.eval = "";
        this.expression = false;
        this.CurrentCalculation.Text = "";
    }


    private void OnSqrt(System.Object sender, EventArgs e)
    {
        if (currentState == 1)
        {
            LockNumberValue(resultText.Text);
            mathOperator = "sqrt";
            currentState = 2;

        }
    }

    private void OnModulo(System.Object sender, EventArgs e)
    {
        if (currentState == 1)
        {
            LockNumberValue(resultText.Text);
            mathOperator = "%";
            currentState = 2;
        }
    }


    public async void OnCalculate(System.Object sender, EventArgs e)
    {
        if (currentState == 2)
        {
            if (secondNumber == 0)
                LockNumberValue(resultText.Text);
            double result = Calculator.Calculate(firstNumber, secondNumber, mathOperator);
            this.CurrentCalculation.Text = $"{firstNumber} {mathOperator} {secondNumber}";
            this.resultText.Text = result.ToTrimmedString(decimalFormat);
            Expressions.Add(CurrentCalculation.Text + " = " + resultText.Text);
            //History.printHistory(Expressions);
            //History.OnCounterClicked1(sender,e);
            for (int i = 0; i < Expressions.Count; i++)
            {
                Item.Name = Expressions[i];
            }
            object value = await database.SaveItemAsync(Item);
            firstNumber = result;
            secondNumber = 0;
            currentState = -1;
            currentEntry = string.Empty;
        }
        else if (expression)
        {
            this.CurrentCalculation.Text = $"{this.eval}";
            this.resultText.Text = this.values.Pop().ToString();
        }
    }


    

    private void OnPercentage(System.Object sender, EventArgs e)
    {
        if (currentState == 1)
        {
            LockNumberValue(resultText.Text);
            decimalFormat = "N2";
            secondNumber = 0.01;
            mathOperator = "×";
            currentState = 2;
            OnCalculate(this, null);
        }
    }

}




