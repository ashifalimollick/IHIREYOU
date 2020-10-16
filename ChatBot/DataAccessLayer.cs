using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace FinanceBot
{
    public class DataAccessLayer
    {
        protected IConfiguration Configuration;

        public DataAccessLayer(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void InsertAnswers(string userid,string answer,string evaluation ,string answertype)
        {
            var str = Configuration["AzureDbConnection"];
            SqlConnection con = new SqlConnection(str);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("dbo.usp_InsertAnswerDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@answers", answer);
                cmd.Parameters.AddWithValue("@answertype", answertype);
                cmd.Parameters.AddWithValue("@evaluation", evaluation);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public void examattended(string mobile)
        {
            var str = Configuration["AzureDbConnection"];
            SqlConnection con = new SqlConnection(str);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("dbo.usp_examattended", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mobile", mobile);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public string FetchType(string mobile,string token)
        {
            string result ="error";
            var str = Configuration["AzureDbConnection"];
            SqlDataReader rdr = null;
            SqlConnection con = new SqlConnection(str);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("dbo.usp_fetchUserType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mobile", mobile);
                cmd.Parameters.AddWithValue("@mailtoken", token);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    result = Convert.ToString(rdr["type"]);
                }
            }
            catch (Exception ex)
            {
                result = "error";
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
            return result;
        }
    }
}
