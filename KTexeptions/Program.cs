using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Bogus;

[Serializable]
class AgeException : Exception
{
    public AgeException() { }
    public AgeException(string message) : base(message) { }
    public AgeException(string message, Exception ex) : base(message) { }
    protected AgeException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext contex)
        : base(info, contex) { }
}
public class Person
{
    public string Name { get; set; }
    public string SecondName { get; set; }
    public int Age { get; set; }
}
class Program
{
    static bool tryy(int age)
    {
        if (age < 15)
        {
            AgeException exc = new AgeException("Регистрация не возможна. Не пройдены возрастные ограничения. (age <= 14)");
            Console.WriteLine(exc);
            //throw exc;
        }
        return true;
    }
    static void Main(string[] args)
    {
        //TestConnection();
        InsertRecord();
        Console.ReadKey();
    }
    private static void InsertRecord()
    {
        using(NpgsqlConnection con =GetConnection())
        {
            try
            {

                var faker = new Faker<Person>()
                    .RuleFor(x => x.Name, f => f.Name.FirstName())
                    .RuleFor(x => x.SecondName, f => f.Name.LastName())
                    .RuleFor(x => x.Age, f => f.Random.Int(10, 20));

                //Faker faker = new Faker("ru");
                //string name = faker.Name.FirstName();
                //string second_name = faker.Name.LastName();
                //Random rnd = new Random();
                //int age = rnd.Next(10, 20);
                var person = faker.Generate(10);
                foreach(var item in person)
                {
                    if (item.Age < 15)
                    {
                        bool result = tryy(item.Age);
                        Console.WriteLine(result);
                    }
                    else
                    {
                        string query = $@"insert into public.users(Name_users, SecondName_users, Age_users)values('{item.Name}','{item.SecondName}', '{item.Age}')";
                        NpgsqlCommand cmd = new NpgsqlCommand(query, con);
                        con.Open();
                        int n = cmd.ExecuteNonQuery();
                        if (n == 1)
                        {
                            Console.WriteLine("Complete");
                        }
                        con.Close();
                    }
                }

            }
            catch (AgeException)
            {
                Console.WriteLine("Регистрация не возможна. Не пройдены возрастные ограничения. (age <= 14)");
                InsertRecord();
            }

        }
    }
    private static void TestConnection()
    {
        using(NpgsqlConnection con=GetConnection())
        {
            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Connected");
            }
        }
    }
    private static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(@"Server=localhost;Port=5432;User Id=postgres;Password=123;Database=KTexceptionsDB;");

    }
}
