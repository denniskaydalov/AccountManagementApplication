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
using System.IO;

namespace WpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //location of the username and password file
        const string FilePath = @"C:\Users\Dennis\source\repos\WpfApplication\WpfApplication\LocalUsernames.txt";
        private bool SignedIn;
        private bool removePressedOnce;

        List<Person> People = new();
        public MainWindow()
        {
            InitializeComponent();
            InitializeProject();
        }

        public void InitializeProject()
        {
            manageFiles();

            myTextBoxUsername.MaxLength = 16;
            myUsernameSignIn.MaxLength = 16;

            myPasswordBox.MaxLength = 16;
            myPasswordSignIn.MaxLength = 16;

            myCurrentAccount.Content = "N/A";

            myCancel.Visibility = Visibility.Hidden;
            myRemoveAccount.Visibility = Visibility.Hidden;
            mySignOut.Visibility = Visibility.Hidden;

            SignedIn = false;
            removePressedOnce = false;         
        }

        private async void manageFiles ()
        {
            if (File.Exists(FilePath) == false)
            {
                //create a file if a file doesnt exist yet
                File.CreateText(FilePath);
            }

            else
            {
                string[] fileLines = await ReadAllLinesAsync(FilePath);

                for (int i = 0; i < fileLines.Length / 2; i++)
                {
                    //add file info to the People list
                    People.Add(new Person { username = fileLines[i * 2 + 1], password = fileLines[i * 2 + 2] });
                    myDropdownMenu.Items.Add(People[i].username);
                }
            }
        }

        public static async Task<string[]> ReadAllLinesAsync (string path)
        { 
            //return the files information through async
            return await File.ReadAllLinesAsync(path);
        }

        private void myConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            //checks if there is no empty space in the boxes and if its not a duplicate
            if (myTextBoxUsername.Text != "" && myPasswordBox.Password != "" && People.Any(p => p.username == myTextBoxUsername.Text) == false)
            {
                //add information of the username and password to the People list and usernames file
                People.Add(new Person { username = myTextBoxUsername.Text, password = myPasswordBox.Password});
                
                File.AppendAllText(FilePath, Environment.NewLine + myTextBoxUsername.Text.ToString());
                File.AppendAllText(FilePath, Environment.NewLine + myPasswordBox.Password);

                //add username information to the dropdown
                myDropdownMenu.Items.Add(People.Last().username);

                myTextBoxUsername.Clear();
                myPasswordBox.Clear();
            }
        }

        private async void mySignInButton_Click(object sender, RoutedEventArgs e)
        {
            string[] fileLines = await ReadAllLinesAsync(FilePath);

            //checks if username is valid 
            if (fileLines.Contains(myUsernameSignIn.Text)  && (Array.FindIndex(fileLines, p => p.Contains(myUsernameSignIn.Text)) + 1 ) % 2 == 0)
            {
                //checks if password is valid
                if (fileLines[Array.FindIndex(fileLines, p => p.Contains(myUsernameSignIn.Text)) + 1] == myPasswordSignIn.Password && SignedIn == false)
                {
                    myCurrentAccount.Content = myUsernameSignIn.Text;

                    SignedIn = true;

                    // adds the remove and signout buttons
                    myRemoveAccount.Visibility = Visibility.Visible;
                    mySignOut.Visibility = Visibility.Visible;

                    myPasswordSignIn.Clear();
                    myUsernameSignIn.Clear();
                }
            }
        }

        private void mySignOut_Click(object sender, RoutedEventArgs e)
        {
            //clear signout info
            SignedIn = false;

            //removes the remove and signout buttons
            myRemoveAccount.Visibility = Visibility.Hidden;
            mySignOut.Visibility = Visibility.Hidden;

            myCurrentAccount.Content = "N/A";
        }

        private async void myRemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            //create lists to work with 
            string[] fileLines = await ReadAllLinesAsync(FilePath);
            List<string> fileLinesList = fileLines.ToList();

            //checks if it's the first time user pressed the remove button
            if (removePressedOnce == false && SignedIn == true)
            {
                //adds the cancel opportunity
                removePressedOnce = true;

                myRemoveAccount.Content = "Are you sure?";

                myCancel.Visibility = Visibility.Visible;
                mySignOut.Visibility = Visibility.Hidden;

            }

            else if (SignedIn == true)
            {            
                //remove the username and password from the People list
                People.RemoveAt(People.FindIndex(p => p.username == myCurrentAccount.Content.ToString()));

                //remove the username and password from the filelineslist
                fileLinesList.Remove(fileLines[Array.FindIndex(fileLines, p => p.Contains(myCurrentAccount.Content.ToString())) + 1]);
                fileLinesList.Remove(myCurrentAccount.Content.ToString());

                //empty the text file
                File.WriteAllText(FilePath, String.Empty);

                //refill the text file with the new list
                for (int i = 0; i < fileLinesList.Count; i++)
                {
                    if (fileLinesList[i] == String.Empty)
                    {
                        continue;
                    }
                    File.AppendAllText(FilePath, Environment.NewLine + fileLinesList[i]);
                }

                //empty sign in information
                myCurrentAccount.Content = String.Empty;
                SignedIn = false;

                //renew dropdown information
                myDropdownMenu.Items.Clear();
                for (int i = 0; i < People.Count; i++)
                {
                    myDropdownMenu.Items.Add(People[i].username);
                }
            }
        }

        private void myCancel_Click(object sender, RoutedEventArgs e)
        {
            //checks if the user knows he can cancel and he has the option to
            if (removePressedOnce == true)
            {
                removePressedOnce = false;

                myCancel.Visibility = Visibility.Hidden;
                mySignOut.Visibility = Visibility.Visible;

                myRemoveAccount.Content = "Remove account";
            }
        }

        private void myDropdownMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            myUsernameSignIn.Text = myDropdownMenu.SelectedItem.ToString();
        }
    }
}
