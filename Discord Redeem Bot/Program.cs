using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data;
using System.Data.Linq;
using System.Net;

public class UserSubscription
{

    public DateTime endTime;
    public ulong userID;

    public ulong mainRoleID;
    public ulong secondRoleID;
 
    public UserSubscription(ulong userID, DateTime endTime, ulong mainRoleID, ulong secondRoleID)
    {
        this.endTime = endTime;
        this.userID = userID;
        this.mainRoleID = mainRoleID;
        this.secondRoleID = secondRoleID;
    }
}
public class RoleWithCodes
{
    public ulong MainRoleID;
    public ulong timeRoleID;
    public string roleCode;

    public RoleWithCodes(ulong roleID, ulong timeRoleID, string roleCode)
    {
        this.MainRoleID = roleID;
        this.timeRoleID = timeRoleID;
        this.roleCode = roleCode;
    }
}


public class User
{
    public string UserID;
    public int CreditCount;
    public bool isMoneyDeposited;
    public int SubscriptionsCount;
    public int BumpedCount;
    public int InvitedUserCount;
    public SqlDataReader dr;
    public SqlCommand cmd;
    private string temp;
    private SqlConnection sql;
    private string temp2;
    public User(ulong UserID, SqlConnection sql)
    {
        string temp = $"SELECT * from UserDatabase where UserID={UserID}";
        this.temp = temp;
        
        this.sql = sql;
        cmd = new SqlCommand(temp, sql);
        this.dr = cmd.ExecuteReader();
        
        if(dr.Read() && dr != null)
        {
          //  CreditCount = GetCreditCount(dr);
          //  isMoneyDeposited = GetMoneyDeposited();
          //  SubscriptionsCount = GetSubscriptionCount();
          //  BumpedCount = GetBumpedCount();
          //  InvitedUserCount = GetInvitedUserCount();
            this.UserID = GetID();
        }
        else
        {
            Console.WriteLine("User not found.");
        }


        Console.WriteLine(UserID);

        dr.Close();

        cmd.Cancel();

    }


    #region Get Methods
    public string GetID()
    {


        try
        {
            string id = dr["UserID"].ToString();
          
            return id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }


    }
    /// <summary>
    /// If credit count found, returns num. Else returns -1.
    /// </summary>
    /// <param name="dr"></param>
    /// <returns></returns>
    public int GetCreditCount(SqlDataReader dr)
    {
       

        try
        {
          
            
          
            var credit = (int)dr["CreditCount"];

            return credit;
        }
        catch (Exception ex)
        {
            Console.WriteLine(" Böcük " +ex.Message);
            return -1;
        }


    }
    /// <summary>
    /// If is deposited found returns true or false. Else returns false.
    /// </summary>
    /// <param name="dr"></param>
    /// <returns></returns>
    public bool GetMoneyDeposited(SqlDataReader dr)
    {


        try
        {
            var isDeposited = false;
            if (dr["isMoneyDeposited"].ToString() == "0")
            {
                isDeposited = false;
            }
            else
            {
                isDeposited = true;
            }

            return isDeposited;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }


    }

    /// <summary>
    /// If Subscription count found returns num. Else returns -1
    /// </summary>
    /// <param name="dr"></param>
    /// <returns></returns>
    public int GetSubscriptionCount(SqlDataReader dr)
    {


        try
        {
            int subCount = int.Parse(dr["SubscriptionsCount"].ToString());

            return subCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }


    }
    /// <summary>
    /// If invited user count found, returns num. Else returns -1.
    /// </summary>
    /// <param name="dr"></param>
    /// <returns></returns>
    public int GetInvitedUserCount(SqlDataReader dr)
    {


        try
        {
            int subCount = int.Parse(dr["InvitedUserCount"].ToString());

            return subCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }


    }
    /// <summary>
    /// If bumped count found, returns num. Else returns -1.
    /// </summary>
    /// <param name="dr"></param>
    /// <returns></returns>
    public int GetBumpedCount(SqlDataReader dr)
    {


        try
        {
            int subCount = int.Parse(dr["BumpedCount"].ToString());

            return subCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }


    }
    #endregion


    #region Get User Methods
    private SqlDataReader GetUser(SqlConnection sql, ulong userID)
    {

        string user = $"SELECT * from UserDatabase where UserID={userID}";
        try
        {
            SqlCommand command = new SqlCommand(user, sql);
            SqlDataReader dr = command.ExecuteReader();
            return dr;
        }
        catch (Exception ex)
        {
            
            Console.WriteLine(ex.Message);
            return null;
        }

        
    }
    #endregion

    #region Set Methods

    public  async Task SetCreditCount(int creditCount)
    {
        try
        {
           
            string code = "update UserDatabase set CreditCount = '" + creditCount + "' where UserID = '" + UserID + "' ";
            SqlCommand cmd = new SqlCommand(code, sql);

            if (sql.State != System.Data.ConnectionState.Open) 
            {
                sql.Open();
            }
            await cmd.ExecuteNonQueryAsync();






            cmd.Cancel();
            Console.WriteLine(temp);
            var cmd2 = new SqlCommand(temp, sql);
            var drr = await cmd2.ExecuteReaderAsync();

            if(drr.Read())
            {
                CreditCount = GetCreditCount(drr);
            }
            else
            {
                Console.WriteLine("Allahını sikerim");
            }
          

          
            drr.Close();


        }
        catch (Exception ex)
        {
            Console.WriteLine("Credit count was not set because: " + ex.Message);
        }
        
    }
    #endregion
}




namespace Discord_Redeem_Bot
{
    internal static class Program
    {
        static string conString = "";
        static SqlConnection sql = new SqlConnection(conString);
        public static DiscordSocketClient _client;
        public static EmbedFooterBuilder SignFooter;
        public static string codesPath;
        public static List<UserSubscription> subscriptions = new List<UserSubscription>();
        public static List<RoleWithCodes> rolesList = new List<RoleWithCodes>();
        public static bool isLoopChecking = false;
        public static string subscriptionFilePath;


        static void Main(string[] args)
        {
            MainAsync().Wait();

        }


        #region Methods
        /// <summary>
        /// If user found, returns SqlDataReader. Else returns null.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static SqlDataReader GetUser(this SqlConnection sql,ulong userID)
        {

            string user = $"SELECT * from UserDatabase where UserID={userID}";
            try
            {
                SqlCommand command = new SqlCommand(user, sql);
                SqlDataReader dr = command.ExecuteReader();
                return dr;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
           
            
        }

        /// <summary>
        /// If user found, returns SqlDataReader. Else returns null.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static SqlDataReader GetUser(ulong userID)
        {

            string user = $"SELECT * from UserDatabase where UserID={userID}";
            try
            {
                SqlCommand command = new SqlCommand(user, sql);
                SqlDataReader dr = command.ExecuteReader();
                return dr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }


        }
        /// <summary>
        /// If id found returns string. Else returns null.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static string GetID(this SqlDataReader dr)
        {

           
            try
            {
                string id = dr["UserID"].ToString();

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }


        }
        /// <summary>
        /// If credit count found, returns num. Else returns -1.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static int GetCreditCount(this SqlDataReader dr)
        {


            try
            {
                var credit = (int)dr["CreditCount"];

                return credit;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }


        }
        /// <summary>
        /// If is deposited found returns true or false. Else returns false.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static bool GetMoneyDeposited(this SqlDataReader dr)
        {


            try
            {
                var isDeposited = false;
                if (dr["isMoneyDeposited"].ToString() == "0")
                {
                    isDeposited = false;
                }
                else
                {
                    isDeposited = true;
                }

                return isDeposited;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


        }

        /// <summary>
        /// If Subscription count found returns num. Else returns -1
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public  static int GetSubscriptionCount(this SqlDataReader dr)
        {


            try
            {
                int subCount = int.Parse(dr["SubscriptionsCount"].ToString());

                return subCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }


        }
        /// <summary>
        /// If invited user count found, returns num. Else returns -1.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static int GetInvitedUserCount(this SqlDataReader dr)
        {


            try
            {
                int subCount = int.Parse(dr["InvitedUserCount"].ToString());

                return subCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }


        }
        /// <summary>
        /// If bumped count found, returns num. Else returns -1.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static int GetBumpedCount(this SqlDataReader dr)
        {


            try
            {
                int subCount = int.Parse(dr["BumpedCount"].ToString());

                return subCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }


        }
        #endregion
        public static async Task MainAsync()
        {
        

            #region Paths
            await sql.OpenAsync();
            Console.WriteLine(sql.Database);
          
            
            Console.WriteLine(Environment.CurrentDirectory);
            
            codesPath = Environment.CurrentDirectory.Remove(Environment.CurrentDirectory.LastIndexOf(@"\"));
            codesPath = codesPath.Remove(codesPath.LastIndexOf(@"\"));
            codesPath = codesPath.Remove(codesPath.LastIndexOf(@"\"));



            User user = new User(0, sql);
            
            if(user != null)
            {
                Console.WriteLine(user.CreditCount);
                 await user.SetCreditCount(21);
               
               
            }


          

           
            string mainCodesPath = codesPath;
            codesPath = mainCodesPath;
            subscriptionFilePath = codesPath + "\\" + "subscriptionDatabase.txt";

            #endregion

            #region Load Datas
            foreach (var data in File.ReadAllText(subscriptionFilePath).Replace("\r", "\r").Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(data.Trim()))
                {
                    try
                    {
                        var datas = data.Split('$');
                        var sub = new UserSubscription(ulong.Parse(datas[0].Trim()), new DateTime(long.Parse(datas[1].Trim())), ulong.Parse(datas[2].Trim()), ulong.Parse(datas[3].Trim()));

                        subscriptions.Add(sub);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }


            }

            //subscriptions.ForEach(m => Console.WriteLine(m.userID + "\n" + m.endTime.Subtract(DateTime.UtcNow).TotalHours + "\n" +m.mainRoleID + "\n" + m.secondRoleID + "\n")); 
            #endregion
      
           





        
            rolesList = new List<RoleWithCodes>()
     {
          new RoleWithCodes(0, 0, "deluxe_1d"),
          new RoleWithCodes(0, 0, "deluxe_3d"),
          new RoleWithCodes(0, 0, "deluxe_1w"),
          new RoleWithCodes(0, 0, "deluxe_1m"),
          new RoleWithCodes(0, 0, "deluxe_3m"),
          new RoleWithCodes(0, 0, "deluxe_6m"),
          new RoleWithCodes(0, 0, "chegg_1t"),
          new RoleWithCodes(0, 0, "coursehero_1t"),
          new RoleWithCodes(0, 0, "scribd_1t"),
          new RoleWithCodes(0, 0, "studocu_1t"),
     };

            #region Discord Client
            SignFooter = new EmbedFooterBuilder().WithText("Powered by LindaMosep!").WithIconUrl("");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildVoiceStates | GatewayIntents.All

            });


            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, "");
            await _client.StartAsync();
            _client.Ready +=  ReadyHandler;

            _client.MessageReceived += MessageHandler;

            _client.UserJoined += UserJoinHandler;
            #endregion

            #region Key Generator

            /*
             KeyGen("deluxe", "1d", 300, codesPath += "\\deluxe1d.txt");
             codesPath = mainCodesPath;
             KeyGen("deluxe", "3d", 300, codesPath += "\\deluxe3d.txt");
             codesPath = mainCodesPath;
             KeyGen("deluxe", "1w", 300, codesPath += "\\deluxe1w.txt");
             codesPath = mainCodesPath;
             KeyGen("deluxe", "1m", 300, codesPath += "\\deluxe1m.txt");
             codesPath = mainCodesPath;
             KeyGen("deluxe", "3m", 300, codesPath += "\\deluxe3m.txt");
             codesPath = mainCodesPath;
             KeyGen("deluxe", "6m", 300, codesPath += "\\deluxe6m.txt");
             codesPath = mainCodesPath;
            */



            #endregion

            await Task.Delay(-1);




        }

        #region Handlers
        private static Task ReadyHandler()
        {
           
            _=LoopChecker();
            return Task.CompletedTask;
        }
        private static Task MessageHandler(SocketMessage e)
        {
            MessageRecieved(e).Wait();
            return Task.CompletedTask;
        }
        private static Task UserJoinHandler(SocketGuildUser e)
        {
            UserJoined(e).Wait();

            return Task.CompletedTask;
        }
        #endregion

      

        #region Embeds
        public static EmbedBuilder SubscriptionFinished(string mention)
        {
            var embed = new EmbedBuilder();
            embed.Footer = SignFooter;
            embed.Color = Color.Blue;
            embed.ThumbnailUrl = "https://images-ext-2.discordapp.net/external/QpCikgWUsI3RN1cjvXJMC3xZjLR2LJrTO4vPowDfE6s/https/c.tenor.com/_UaFpyE0SPYAAAAC/praying-cat.gif";
            embed.Title = "Your subscription ended!";
            embed.AddField(" Thank you for purchasing our subscription!", "**You can re-subscribe on **" + $"[here](https://shoppy.gg/@Meowdemia)");
            return embed;
        }
        public static EmbedBuilder YouHaveSubscription(SocketUser user, string timeDifference, string RoleName)
        {
            var embed = new EmbedBuilder();
            embed.Footer = SignFooter;
            embed.Color = Color.Purple;
            if (timeDifference != "0")
            {
                timeDifference = TimeDifference(timeDifference);
                embed.ThumbnailUrl = "https://images-ext-2.discordapp.net/external/J2F9YOAsRKxBnEBbxzRt9knkSfG27DBcF-De9fqzLys/%3Fwidth%3D745%26height%3D559/https/images-ext-2.discordapp.net/external/-xCsuG6EsrPfO15glJR33j2U57ztvS432HaW0D0oRV0/https/cdn.dribbble.com/users/1162077/screenshots/5427775/media/612968fb2a4690f4959deb23a00eb2d0.gif";
                embed.Title = "You've already subscribed!";
                embed.WithDescription(user.Mention + $" You've already **{RoleName}** and it'll finish after **{timeDifference}**. If you've any question about subscription, you can contact with our support team!");
            }
            else
            {
                embed.ThumbnailUrl = "https://images-ext-2.discordapp.net/external/J2F9YOAsRKxBnEBbxzRt9knkSfG27DBcF-De9fqzLys/%3Fwidth%3D745%26height%3D559/https/images-ext-2.discordapp.net/external/-xCsuG6EsrPfO15glJR33j2U57ztvS432HaW0D0oRV0/https/cdn.dribbble.com/users/1162077/screenshots/5427775/media/612968fb2a4690f4959deb23a00eb2d0.gif";
                embed.Title = "You already own one time unlock!";
                embed.WithDescription(user.Mention + $" You've already own **{RoleName} one time**. If you've any question about subscription, you can contact with our support team!");
            }

            return embed;
        }
        public static EmbedBuilder NotValidCode(SocketUser user)
        {
            var embed = new EmbedBuilder();
            embed.Footer = SignFooter;
            embed.Color = Color.Red;

            embed.ThumbnailUrl = "https://media.discordapp.net/attachments/917813714923683860/927427593760800778/image_processing20200730-32280-1ke6zr2.gif";
            embed.Title = "Invalid or used code!";
            embed.WithDescription(user.Mention + $"Invalid code. If you think it is a mistake, contact with support team.");
            return embed;
        }
        public static EmbedBuilder SubscriptionStarted(SocketUser user, string timeDifference, string roleName)
        {
            var embed = new EmbedBuilder();
            embed.Footer = SignFooter;
            embed.Color = Color.Green;
            if (timeDifference != "0")
            {

                timeDifference = TimeDifference(timeDifference);

              
                embed.ThumbnailUrl = "https://media.discordapp.net/attachments/917813714923683860/927371329638912010/ba8ae9699902e9734c7de16cbb957a47_1.gif";
                embed.Title = "You subscribed succesfully!";
                embed.WithDescription(user.Mention + $" Your **{roleName}** subscription succesfully started. It'll finish after **{timeDifference}**. Study well!");
            }
            else
            {
                embed.ThumbnailUrl = "https://media.discordapp.net/attachments/917813714923683860/927371329638912010/ba8ae9699902e9734c7de16cbb957a47_1.gif";
                embed.Title = "You got one time unlock succesfully!";
                embed.WithDescription(user.Mention + $" You got **{roleName}** one time succesfully. Study well!");

            }

            return embed;
        }
        public static EmbedBuilder TransferCompletedSuccesfully(SocketRole transRole0, SocketRole transRole1, SocketUser user1, SocketUser user2, List<SocketRole> roles)
        {
            var builder = new EmbedBuilder();
            builder.Title = "Transfer completed succesfully!";
            builder.Description = $"{user1.Mention} Your {transRole0.Mention} has been transferred to {user2.Mention}.";
            builder.ThumbnailUrl = "https://images-ext-2.discordapp.net/external/lWZx6rxH_CFm1TTfy4Y_n3w99c-JpNuu7ZiQv6gA0Ao/https/media.discordapp.net/attachments/917813714923683860/927372219431129148/c3b6e85cfdddd49e731f27c31e4fc5e6_1.gif";
            builder.Footer = SignFooter;
            builder.Color = Color.Green;
            if (roles.Count > 0)
            {
                string onlyHave = " Now you only have ";
                foreach (var role in roles)
                {
                    if (!role.IsEveryone && role.Id != transRole0.Id && role.Id != transRole1.Id)
                    {
                        onlyHave += role.Mention + " ";
                    }
                }

                if (onlyHave != " Now you only have ")
                {
                    builder.Description += "\n" + onlyHave;
                }
                else
                {
                    builder.Description += " \n You no longer have role. :(";
                }
            }
            return builder;
        }
        public static EmbedBuilder TransferCompletedNotificationUser(string transRole, SocketUser user1, string time)
        {
            var builder = new EmbedBuilder();
            builder.Title = "Congratulations!";
            builder.Footer = SignFooter;
            builder.ThumbnailUrl = "https://media.discordapp.net/attachments/917813714923683860/927462600235704340/giphy.gif";
            builder.Description = $"**{user1.Username + "#" + user1.Discriminator}** transferred **{transRole}** to you. This subscription will end after **{time}**.";
            builder.Color =  new Color(255, 215, 0);

            return builder;
        }

        #endregion

        public static async Task UserJoined(SocketGuildUser e)
        {

            if (subscriptions.Find(m => m.userID == e.Id) != null)
            {
                while (isLoopChecking)
                {
                    int m = 0;
                    m++;
                }


                var embed = new EmbedBuilder();
                embed.Footer = SignFooter;
                embed.Color = Color.Gold;
                embed.ThumbnailUrl = "https://media.discordapp.net/attachments/917813714923683860/927460895771230248/Green_Computer_Cat_Christmas_Instagram_Post_1080_x_1500_px_1200_x_1500_px.gif?width=1500&height=1800";
                embed.Title = "Welcome back!";


                var sub = subscriptions.Find(m => m.userID == e.Id);

                var nameOfRole = e.Guild.Roles.ToList().Find(m => m.Id == sub.secondRoleID).Name;
                var timeDifference = sub.endTime.Subtract(DateTime.UtcNow).ToString();

                timeDifference = TimeDifference(timeDifference);
                embed.WithDescription($"You still have your subscription at Meowdemia. I give back your **{nameOfRole}** and your subscription will end after **{timeDifference}**.");
                _=e.AddRolesAsync(new List<ulong> { sub.mainRoleID, sub.secondRoleID });
                await e.SendMessageAsync("", false, embed.Build());
            }

        }
        public static async Task MessageRecieved(SocketMessage e)
        {
            if (!e.Author.IsBot)
            {
                if ((e.Channel as SocketGuildChannel) != null)
                {
                    if(e.Content.StartsWith("!!"))
                    {

                        var content = e.Content.Substring(2);

                        if(content.ToLower() == "credit" && content.ToLower() == "c")
                        {
                           var user =  GetUser(e.Author.Id);
                            if(user != null)
                            {
                                 
                            }
                            else
                            {
                             
                            }
                        }
                    }
                    if (_client.GetGuild(0).GetUser(e.Author.Id) != null)
                    {
                        if (e.Content.StartsWith("!r "))
                        {
                            if (e.Content.Contains("chegg_"))
                            {
                                var baseCode = e.Content.Substring(e.Content.IndexOf("chegg_"));
                                CodeChecker(e, baseCode);
                            }
                            else if (e.Content.Contains("coursehero_"))
                            {
                                var baseCode = e.Content.Substring(e.Content.IndexOf("coursehero_"));
                                CodeChecker(e, baseCode);
                            }
                            else if (e.Content.Contains("scribd_"))
                            {
                                var baseCode = e.Content.Substring(e.Content.IndexOf("scribd_"));
                                CodeChecker(e, baseCode);
                            }
                            else if (e.Content.Contains("studocu_"))
                            {
                                var baseCode = e.Content.Substring(e.Content.IndexOf("studocu_"));
                                CodeChecker(e, baseCode);
                            }
                            else if (e.Content.Contains("deluxe_"))
                            {
                                var baseCode = e.Content.Substring(e.Content.IndexOf("deluxe_"));
                                CodeChecker(e, baseCode);
                            }

                        }
                        if(e.Content.StartsWith("!remove sub"))
                        {
                            var user = _client.GetGuild(0).GetUser(e.Author.Id) as IGuildUser;

                            if(subscriptions.Find(m => m.userID == e.Author.Id) != null)
                            {
                                subscriptions.Find(m => m.userID == e.Author.Id).endTime = DateTime.UtcNow;
                                

                            }
                        }
                    }

                }
                else
                {
                    if (e.Content.StartsWith("!t sub"))
                    {
                        if (e.MentionedUsers.Count == 1)
                        {
                            if (!e.MentionedUsers.ToList()[0].IsBot)
                            {
                                if (subscriptions.Find(m => m.userID == e.Author.Id) != null)
                                {
                                    if (subscriptions.Find(m => m.userID == e.MentionedUsers.ToList()[0].Id) == null)
                                    {
                                        var sub = subscriptions.Find(m => m.userID == e.Author.Id);
                                        var subscriptionFilePath = codesPath + "\\" + "subscriptionDatabase.txt";

                                        subscriptions.Find(m => m.userID == e.Author.Id).userID = e.MentionedUsers.ToList()[0].Id;

                                        File.WriteAllText(subscriptionFilePath, File.ReadAllText(subscriptionFilePath).Replace(e.Author.Id.ToString(), e.MentionedUsers.ToList()[0].Id.ToString()));

                                        var user1 = e.Author as IGuildUser;
                                        var user2 = _client.GetGuild(0).GetUser(e.MentionedUsers.ToList()[0].Id);

                                        if (user2 != null)
                                        {
                                            var transferredRole = _client.GetGuild(0).GetRole(sub.secondRoleID);
                                            var transferredRole1 = _client.GetGuild(0).GetRole(sub.mainRoleID);

                                            await user1.RemoveRolesAsync(new List<ulong> { sub.mainRoleID, sub.secondRoleID });
                                            await user2.AddRolesAsync(new List<ulong> { sub.mainRoleID, sub.secondRoleID });

                                            _=e.Channel.SendMessageAsync("", false, TransferCompletedSuccesfully(transferredRole, transferredRole1, user1 as SocketGuildUser, user2 as SocketGuildUser, (user1 as SocketGuildUser).Roles.ToList()).Build(), null, AllowedMentions.None);

                                            _=user2.SendMessageAsync("", false, TransferCompletedNotificationUser(transferredRole.Name, user1 as SocketGuildUser, TimeDifference(sub.endTime.Subtract(DateTime.UtcNow).ToString())).Build());
                                        }
                                       
                                    }
                                }
                            }
                        }

                    }
                }
            }
           
        }
        public static async void CodeChecker(SocketMessage e, string baseCode)
        {

            string code = e.Content.Substring(e.Content.IndexOf(baseCode));
            string typeOfCode = code.Remove(code.IndexOf("_"));
            string timeOfCode = code.Substring(code.IndexOf("_") + 1);
            timeOfCode = timeOfCode.Remove(timeOfCode.IndexOf("."));



            string FilePath = codesPath + "\\" + typeOfCode + timeOfCode + ".txt";

            string CodeList = "";
            if (File.Exists(FilePath))
            {
                CodeList = File.ReadAllText(FilePath);
            }

            var user2 = _client.GetGuild(0).Users.ToList().Find(m => m.Id == e.Author.Id);
            var subscriptionFilePath = codesPath + "\\" + "subscriptionDatabase.txt";

            if (user2 != null)
            {
                var user = _client.GetGuild(0).Users.ToList().Find(m => m.Id == e.Author.Id) as IGuildUser;
                //First check user roles
                if (user.RoleIds.ToList().Contains(0))
                {
                    await e.Author.SendMessageAsync("", false, YouHaveSubscription(e.Author, subscriptions.Find(m => m.userID == e.Author.Id).endTime.Subtract(DateTime.UtcNow).ToString(), user2.Roles.ToList().Find(m => m.Id == subscriptions.Find(t => t.userID == e.Author.Id).secondRoleID).Name).Build());
                }
                else
                {
                    var role = rolesList.Find(m => m.roleCode == typeOfCode + "_" + timeOfCode);
                    if(role != null)
                    {
                        if (user.RoleIds.Contains(role.timeRoleID))
                        {

                            await e.Author.SendMessageAsync("", false, YouHaveSubscription(e.Author, "0", user2.Roles.ToList().Find(m => m.Id == role.MainRoleID).Name).Build());

                        }
                        else
                        {
                            if (!CodeList.Contains(code))
                            {
                               _=e.Author.SendMessageAsync("", false, NotValidCode(e.Author).Build());
                            }
                            else
                            {
                                if (rolesList.Find(m => m.roleCode == typeOfCode + "_" + timeOfCode) != null)
                                {

                                    if (role != null)
                                    {
                                        if (user.RoleIds.Contains(role.timeRoleID))
                                        {

                                            await e.Author.SendMessageAsync("", false, YouHaveSubscription(e.Author, "0", user2.Roles.ToList().Find(m => m.Id == subscriptions.Find(t => t.userID == e.Author.Id).secondRoleID).Name).Build());

                                        }
                                        else
                                        {
                                            if (!role.roleCode.Contains("deluxe"))
                                            {
                                                List<ulong> roleIds = new List<ulong>
                                         {
                                             role.timeRoleID,
                                             role.MainRoleID
                                         };


                                                _=user.AddRolesAsync(roleIds);
                                                File.WriteAllText(FilePath, File.ReadAllText(FilePath).Replace(code, "\n"));
                                                string text = File.ReadAllText(FilePath);
                                                var result = Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline).TrimEnd();
                                                File.WriteAllText(FilePath, result);

                                                await e.Author.SendMessageAsync("", false, SubscriptionStarted(user2, "0", _client.GetGuild(0).Roles.ToList().Find(m => m.Id == role.MainRoleID).Name).Build());

                                            }
                                            else
                                            {

                                                File.WriteAllText(FilePath, File.ReadAllText(FilePath).Replace(code, "\n"));
                                                string text = File.ReadAllText(FilePath);
                                                var result = Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline).TrimEnd();
                                                File.WriteAllText(FilePath, result);

                                                List<ulong> roleIds = new List<ulong>
                                        {
                                            role.timeRoleID,
                                            role.MainRoleID
                                        };

                                                _=user.AddRolesAsync(roleIds);

                                                DateTime time = DateTime.Now;
                                                if (timeOfCode.Contains("1d"))
                                                {
                                                    time = DateTime.UtcNow.AddSeconds(30);
                                                }
                                                else if (timeOfCode.Contains("3d"))
                                                {
                                                    time = DateTime.UtcNow.AddDays(3);
                                                }
                                                else if (timeOfCode.Contains("1w"))
                                                {
                                                    time = DateTime.UtcNow.AddDays(7);
                                                }
                                                else if (timeOfCode.Contains("1m"))
                                                {
                                                    time = DateTime.UtcNow.AddDays(30);
                                                }
                                                else if (timeOfCode.Contains("3m"))
                                                {
                                                    time = DateTime.UtcNow.AddDays(90);
                                                }
                                                else if (timeOfCode.Contains("6m"))
                                                {
                                                    time = DateTime.UtcNow.AddDays(180);
                                                }



                                                subscriptions.Add(new UserSubscription(e.Author.Id, time, role.MainRoleID, role.timeRoleID));
                                                File.WriteAllText(subscriptionFilePath, File.ReadAllText(subscriptionFilePath) + $" {e.Author.Id} $ {time.Ticks} $ {role.MainRoleID} $ {role.timeRoleID}\n");
                                                await e.Author.SendMessageAsync("", false, SubscriptionStarted(user2, time.Subtract(DateTime.UtcNow).ToString(), _client.GetGuild(0).Roles.ToList().Find(m => m.Id == role.timeRoleID).Name).Build());

                                            }
                                        }
                                    }
                                    else
                                    {
                                        _=e.Author.SendMessageAsync("", false, NotValidCode(e.Author).Build());
                                    }



                                }
                            }
                        }
                    }
                    else
                    {
                        _=e.Author.SendMessageAsync("", false, NotValidCode(e.Author).Build());
                    }


                }


            }

        }
        public static async Task LoopChecker()
        {
            isLoopChecking = true;
            var subscriptionFilePath = codesPath + "\\" + "subscriptionDatabase.txt";
            List<UserSubscription> removeUser = new List<UserSubscription>();
            foreach (var m in subscriptions)
            {

                if (DateTime.Compare(DateTime.UtcNow, m.endTime) == 0 || DateTime.Compare(DateTime.UtcNow, m.endTime) == 1)
                {
                    removeUser.Add(m);

                }
            }

            foreach (var m in removeUser)
            {

                try
                {

                  var userr =   _client.GetGuild(0).Users.ToList().Find(t => t.Id == m.userID && (t as IGuildUser).RoleIds.Contains(m.mainRoleID)  && (t as IGuildUser).RoleIds.Contains(m.secondRoleID));

                    if(userr != null)
                    {
                        _=userr.RemoveRolesAsync(new List<ulong> { m.mainRoleID, m.secondRoleID });
                    }

                    string text = File.ReadAllText(subscriptionFilePath);
                    text = text.Remove(text.IndexOf(m.userID.ToString()), text.Substring(text.IndexOf(m.userID.ToString())).IndexOf(m.secondRoleID.ToString()) + m.secondRoleID.ToString().Length);
                    var result = Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline).TrimEnd();
                    File.WriteAllText(subscriptionFilePath, result);
                    await _client.GetGuild(0).Users.ToList().Find(t => t.Id == m.userID).SendMessageAsync("", false, SubscriptionFinished(_client.GetGuild(0).Users.ToList().Find(t => t.Id == m.userID).Mention).Build());


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }





            subscriptions.RemoveAll(x => removeUser.Contains(x));

            isLoopChecking = false;
            await Task.Delay(TimeSpan.FromSeconds(5));
            await LoopChecker();
        }

        public static string TimeDifference(string timeDifference)
        {
            int dotCount = 0;
            foreach (var m in timeDifference.ToCharArray())
            {
                if (m == '.')
                {
                    dotCount++;
                }
            }


            if (dotCount > 0)
            {

                if (dotCount == 1)
                {

                    if (timeDifference.Substring(timeDifference.IndexOf(":")).IndexOf(".") == -1)
                    {
                        timeDifference = timeDifference.Insert(timeDifference.IndexOf("."), " Day and ");
                        timeDifference = timeDifference.Remove(timeDifference.IndexOf("."), 1);
                    }
                    else
                    {

                        timeDifference = timeDifference.Remove(timeDifference.IndexOf("."), 1);
                    }
                }
                else if (dotCount > 1)
                {

                    if (timeDifference.Substring(timeDifference.IndexOf(":")).IndexOf(".") == -1)
                    {
                        Console.WriteLine("b");
                        timeDifference = timeDifference.Insert(timeDifference.IndexOf("."), " Day and ");
                        timeDifference = timeDifference.Remove(timeDifference.IndexOf("."), 1);


                    }
                    else
                    {


                        if (timeDifference.Remove(timeDifference.IndexOf(":")).IndexOf(".") != -1)
                        {

                            timeDifference = timeDifference.Insert(timeDifference.IndexOf("."), " Day and ");
                            timeDifference = timeDifference.Remove(timeDifference.IndexOf("."), 1);

                            while (timeDifference.IndexOf(".") != -1)
                            {

                                timeDifference = timeDifference.Remove(timeDifference.IndexOf("."));
                            }


                        }
                        else
                        {

                            while (timeDifference.IndexOf(".") != -1)
                            {
                                timeDifference = timeDifference.Remove(timeDifference.IndexOf("."));
                            }
                        }

                    }
                }

            }

            if (timeDifference.Substring(timeDifference.LastIndexOf(":")).Length > 3)
            {
                timeDifference = timeDifference.Remove(timeDifference.LastIndexOf(":") + 3);
            }

            if (timeDifference.Contains(" and 00:00:00"))
            {
                timeDifference = timeDifference.Replace(" and 00:00:00", "");
            }

            return timeDifference;
        }
        public static void KeyGen(string service, string type, int count, string FileName)
        {
            var alphabet = "qwertyuopasdfghjklizxcvbnm<>%#&^+£$0123456789".ToCharArray();
            Random rand = new Random();
            Random rand2 = new Random();
            for (int m = 0; m < count; m++)
            {
                if (m == 0)
                {
                    File.WriteAllText(FileName, "");
                }
                if (service == "chegg")
                {

                    string txt = File.ReadAllText(FileName);
                    string code = "chegg_";

                    if (type == "1t")
                    {
                        code += "1t.";

                    }
                    else if (type == "1d")
                    {
                        code += "1d.";
                    }
                    else if (type == "3d")
                    {
                        code += "3d.";
                    }
                    else if (type == "1w")
                    {
                        code += "1w.";
                    }
                    else if (type == "1m")
                    {
                        code += "1m.";
                    }
                    else if (type == "3m")
                    {
                        code += "3m.";
                    }
                    else if (type == "6m")
                    {
                        code += "6m.";
                    }

                    for (int i = 0; i < 18; i++)
                    {

                        int random = rand.Next(0, alphabet.Length);
                        int randomForUpper = rand2.Next(0, 2);
                        if (randomForUpper == 0)
                        {
                            code += alphabet[random];
                        }
                        else
                        {
                            code += alphabet[random].ToString().ToUpper();
                        }



                    }

                    while (txt.Contains(code))
                    {
                        int random = rand.Next(0, alphabet.Length);
                        code += alphabet[random];
                    }

                    txt += code + "\n";
                    File.WriteAllText(FileName, txt);

                }
                else if (service == "coursehero")
                {

                    string txt = File.ReadAllText(FileName);
                    string code = "coursehero_";

                    if (type == "1t")
                    {
                        code += "1t.";

                    }
                    else if (type == "1d")
                    {
                        code += "1d.";
                    }
                    else if (type == "3d")
                    {
                        code += "3d.";
                    }
                    else if (type == "1w")
                    {
                        code += "1w.";
                    }
                    else if (type == "1m")
                    {
                        code += "1m.";
                    }
                    else if (type == "3m")
                    {
                        code += "3m.";
                    }
                    else if (type == "6m")
                    {
                        code += "6m.";
                    }

                    for (int i = 0; i < 18; i++)
                    {

                        int random = rand.Next(0, alphabet.Length);
                        int randomForUpper = rand2.Next(0, 2);
                        if (randomForUpper == 0)
                        {
                            code += alphabet[random];
                        }
                        else
                        {
                            code += alphabet[random].ToString().ToUpper();
                        }



                    }

                    while (txt.Contains(code))
                    {
                        int random = rand.Next(0, alphabet.Length);
                        code += alphabet[random];
                    }

                    txt += code + "\n";
                    File.WriteAllText(FileName, txt);
                }
                else if (service == "scribd")
                {

                    string txt = File.ReadAllText(FileName);
                    string code = "scribd_";
                    if (type == "1t")
                    {
                        code += "1t.";
                    }
                    else if (type == "1d")
                    {
                        code += "1d.";
                    }
                    else if (type == "3d")
                    {
                        code += "3d.";
                    }
                    else if (type == "1w")
                    {
                        code += "1w.";
                    }
                    else if (type == "1m")
                    {
                        code += "1m.";
                    }
                    else if (type == "3m")
                    {
                        code += "3m.";
                    }
                    else if (type == "6m")
                    {
                        code += "6m.";
                    }

                    for (int i = 0; i < 18; i++)
                    {

                        int random = rand.Next(0, alphabet.Length);
                        int randomForUpper = rand2.Next(0, 2);
                        if (randomForUpper == 0)
                        {
                            code += alphabet[random];
                        }
                        else
                        {
                            code += alphabet[random].ToString().ToUpper();
                        }



                    }

                    while (txt.Contains(code))
                    {
                        int random = rand.Next(0, alphabet.Length);
                        code += alphabet[random];
                    }

                    txt += code + "\n";
                    File.WriteAllText(FileName, txt);
                }
                else if (service == "studocu")
                {

                    string txt = File.ReadAllText(FileName);
                    string code = "studocu_";

                    if (type == "1t")
                    {
                        code += "1t.";

                    }
                    else if (type == "1d")
                    {
                        code += "1d.";
                    }
                    else if (type == "3d")
                    {
                        code += "3d.";
                    }
                    else if (type == "1w")
                    {
                        code += "1w.";
                    }
                    else if (type == "1m")
                    {
                        code += "1m.";
                    }
                    else if (type == "3m")
                    {
                        code += "3m.";
                    }
                    else if (type == "6m")
                    {
                        code += "6m.";
                    }

                    for (int i = 0; i < 18; i++)
                    {

                        int random = rand.Next(0, alphabet.Length);
                        int randomForUpper = rand2.Next(0, 2);
                        if (randomForUpper == 0)
                        {
                            code += alphabet[random];
                        }
                        else
                        {
                            code += alphabet[random].ToString().ToUpper();
                        }



                    }

                    while (txt.Contains(code))
                    {
                        int random = rand.Next(0, alphabet.Length);
                        code += alphabet[random];
                    }

                    txt += code + "\n";
                    File.WriteAllText(FileName, txt);
                }
                else if (service == "deluxe")
                {
                    string txt = File.ReadAllText(FileName);
                    string code = "deluxe_";


                    if (type == "1d")
                    {
                        code += "1d.";
                    }
                    else if (type == "3d")
                    {
                        code += "3d.";
                    }
                    else if (type == "1w")
                    {
                        code += "1w.";
                    }
                    else if (type == "1m")
                    {
                        code += "1m.";
                    }
                    else if (type == "3m")
                    {
                        code += "3m.";
                    }
                    else if (type == "6m")
                    {
                        code += "6m.";
                    }

                    for (int i = 0; i < 18; i++)
                    {

                        int random = rand.Next(0, alphabet.Length);
                        int randomForUpper = rand2.Next(0, 2);
                        if (randomForUpper == 0)
                        {
                            code += alphabet[random];
                        }
                        else
                        {
                            code += alphabet[random].ToString().ToUpper();
                        }



                    }

                    while (txt.Contains(code))
                    {
                        int random = rand.Next(0, alphabet.Length);
                        code += alphabet[random];
                    }

                    txt += code + "\n";
                    File.WriteAllText(FileName, txt);

                }

            }


        }
        static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
