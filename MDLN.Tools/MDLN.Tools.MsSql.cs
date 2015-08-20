using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace MDLN.Tools {
	/// <summary>
	/// A class similar to dictionaries that will use a string index with a variant data type.
	/// The class will automatically convert the data to the available types; string, integer, boolean, and string array.
	/// </summary>
	public class DataRecord : IEnumerable {
		/// <summary>
		/// Dictionary housing all values stored in this record
		/// </summary>
		protected Dictionary<string, DataValue> cdctValues;

		/// <summary>
		/// Constructor to prepare the class
		/// </summary>
		public DataRecord() {
			cdctValues = new Dictionary<string, DataValue>();
		}

		/// <summary>
		/// Retrieve an indexed value as a string
		/// </summary>
		/// <param name="strIndex">Index name of the value</param>
		/// <returns>String version of the stored value</returns>
		public virtual string this[string strIndex] {
			get {
				return cdctValues[strIndex].String;
			}

			set {
				cdctValues[strIndex].String = value;
			}
		}

		/// <summary>
		/// Tests to see if the specified key exists in the datarecord
		/// </summary>
		/// <param name="strKey">The key to test for</param>
		/// <returns>True if the key exists, false otherwise</returns>
		public Boolean ContainsKey(string strKey) {
			return cdctValues.ContainsKey(strKey);
		}

		/// <summary>
		/// Retrieves the number of indexed values in this record
		/// </summary>
		/// <returns>Number of values</returns>
		public int KeyCount() {
			return cdctValues.Count;
		}

		/// <summary>
		/// Retrieve an indexed value as a string
		/// </summary>
		/// <param name="strIndex">Index name of the value</param>
		/// <returns>String version of the stored value</returns>
		public string String(string strIndex) {
			DataValue oCurrVal;

			oCurrVal = cdctValues[strIndex];

			return oCurrVal.String;
		}

		/// <summary>
		/// Retrieve an indexed value as a number
		/// </summary>
		/// <param name="strIndex">Index name of the value</param>
		/// <returns>Integer version of the stored value</returns>
		public int Number(string strIndex) {
			DataValue oCurrVal;

			oCurrVal = cdctValues[strIndex];

			return oCurrVal.Number;
		}

		/// <summary>
		/// Retrieve an indexed value as a boolean
		/// </summary>
		/// <param name="strIndex">Index name of the value to retrieve</param>
		/// <returns>Boolean version of the indexed value</returns>
		public bool Boolean(string strIndex) {
			DataValue oCurrVal;

			oCurrVal = cdctValues[strIndex];

			return oCurrVal.Boolean;
		}

		/// <summary>
		/// Add a new value to the index list
		/// </summary>
		/// <param name="strName">Index name to give this value</param>
		/// <param name="strValue">Value to set</param>
		public void AddValue(string strName, string strValue) {
			DataValue oNewValue;

			oNewValue = new DataValue(strValue);

			if (cdctValues.ContainsKey(strName) == true) {  //UPdate existing value
				cdctValues[strName] = oNewValue;
			} else {  //Add new key
				cdctValues.Add(strName, oNewValue);
			}
		}

		/// <summary>
		/// Add a new value to the index list
		/// </summary>
		/// <param name="strName">Index name to give this value</param>
		/// <param name="iValue">Value to set</param>
		public void AddValue(string strName, int iValue) {
			DataValue oNewValue;

			oNewValue = new DataValue(iValue);

			if (cdctValues.ContainsKey(strName) == true) {  //UPdate existing value
				cdctValues[strName] = oNewValue;
			} else {  //Add new key
				cdctValues.Add(strName, oNewValue);
			}
		}

		/// <summary>
		/// Add a new value to the index list
		/// </summary>
		/// <param name="strName">Index name to give this value</param>
		/// <param name="bValue">Value to set</param>
		public void AddValue(string strName, bool bValue) {
			DataValue oNewValue;

			oNewValue = new DataValue(bValue);

			if (cdctValues.ContainsKey(strName) == true) {  //UPdate existing value
				cdctValues[strName] = oNewValue;
			} else {  //Add new key
				cdctValues.Add(strName, oNewValue);
			}
		}

		/// <summary>
		/// Add a new value to the index list
		/// </summary>
		/// <param name="strName">Index name to give this value</param>
		/// <param name="astrValue">Value to set</param>
		public void AddValue(string strName, IEnumerable<string> astrValue) {
			DataValue oNewValue;

			oNewValue = new DataValue(astrValue);

			if (cdctValues.ContainsKey(strName) == true) {  //UPdate existing value
				cdctValues[strName] = oNewValue;
			} else {  //Add new key
				cdctValues.Add(strName, oNewValue);
			}
		}

		/// <summary>
		/// Sets the value for an indexed element
		/// </summary>
		/// <param name="strName">Elements index name</param>
		/// <param name="strValue">New value to set</param>
		public void SetValue(string strName, string strValue) {
			DataValue oValue;

			if (cdctValues.ContainsKey(strName) == false) { //New value, create it
				oValue = new DataValue(strValue);
				cdctValues.Add(strName, oValue);
			} else {
				oValue = cdctValues[strName];
				oValue.String = strValue;
				cdctValues[strName] = oValue;
			}
		}

		/// <summary>
		/// Sets the value for an indexed element
		/// </summary>
		/// <param name="strName">Elements index name</param>
		/// <param name="iValue">New value to set</param>
		public void SetValue(string strName, int iValue) {
			DataValue oValue;

			oValue = cdctValues[strName];
			oValue.Number = iValue;
		}

		/// <summary>
		/// Sets the value for an indexed element
		/// </summary>
		/// <param name="strName">Elements index name</param>
		/// <param name="bValue">New value to set</param>
		public void SetValue(string strName, bool bValue) {
			DataValue oValue;

			oValue = cdctValues[strName];
			oValue.Boolean = bValue;
		}

		/// <summary>
		/// Sets the value for an indexed element
		/// </summary>
		/// <param name="strName">Elements index name</param>
		/// <param name="astrValue">New value to set</param>
		public void SetValue(string strName, IEnumerable<string> astrValue) {
			DataValue oValue;

			oValue = cdctValues[strName];
			oValue.StringList = astrValue;
		}

		/// <summary>
		/// Finds a value by a description of the value
		/// </summary>
		/// <param name="strDescription">The description or name of the value to retrieve</param>
		/// <returns>The value in string format</returns>
		public virtual string FindStringValue(string strDescription) {
			DataValue oCurrVal;

			oCurrVal = cdctValues[strDescription];

			return oCurrVal.String;
		}

		/// <summary>
		/// Compares the data record against a known true record.  Each key or index from each data record is compared to ensure it exists in the other.
		/// </summary>
		/// <param name="datTrueRecord">The known true record</param>
		public void CompareKeysToTrueRecord(DataRecord datTrueRecord) {
			string strLocalKeys, strExternalKeys, strException;

			strException = "";

			//Look through all local keys to see if external data record has them
			strLocalKeys = "";
			foreach (string strKey in this) {
				if (datTrueRecord.ContainsKey(strKey) == false) { //External data record does not have this key
					strLocalKeys = strLocalKeys + strKey + ", ";
				}
			}

			if (strLocalKeys.CompareTo("") != 0) {  //found keys, trim trailing comma
				strLocalKeys = strLocalKeys.Substring(0, strLocalKeys.Length - 2);

				strException = strException + "Test data record (local) contains the following keys which the True data record (external) does not have: " + strLocalKeys;
			}

			//Look through all external keys to see if local record has them
			strExternalKeys = "";
			foreach (string strKey in datTrueRecord) {
				if (this.ContainsKey(strKey) == false) { //External data record has a key that isn't in local
					strExternalKeys = strExternalKeys + strKey + ", ";
				}
			}

			if (strExternalKeys.CompareTo("") != 0) {  //found keys, trim trailing comma
				strExternalKeys = strExternalKeys.Substring(0, strExternalKeys.Length - 2);

				strException = strException + "True data record (external) contains the following keys which the Test data record (local) does not have: " + strLocalKeys;
			}

			if (strException.CompareTo("") != 0) {  //Found discrepencies, throw an exception
				throw new Exception(strException);
			}
		}

		/// <summary>
		/// Compares all values in the data record against the values in another datarecord with known true values
		/// </summary>
		/// <param name="datTrueRecord">The known true record</param>
		public void CompareValuesToTrueRecord(DataRecord datTrueRecord) {
			String strSkippedKeys, strException;

			strException = "";
			strSkippedKeys = "";

			foreach (string strKey in this) {
				if (datTrueRecord.ContainsKey(strKey) == true) {  //True record has this key, compare it
					if (this.String(strKey).CompareTo(datTrueRecord.String(strKey)) != 0) { //Values don't match
						strException = strException + "The values in Key '" + strKey + "' do not match.  Test (local) value: '" + this.String(strKey) + "'  True (external) value: '" + datTrueRecord.String(strKey) + "\n";
					}
				} else { //True record does not have this key report it
					strSkippedKeys = strSkippedKeys + strKey + ", ";
				}
			}

			if (strSkippedKeys.CompareTo("") != 0) {  //skipped keys, trim trailing comma
				strSkippedKeys = strSkippedKeys.Substring(0, strSkippedKeys.Length - 2);

				strException = strException + "Skipped comparing some values, True data record (external) does not contain the following keys: " + strSkippedKeys;
			}

			if (strException.CompareTo("") != 0) {  //Found discrepencies, throw an exception
				throw new Exception(strException);
			}
		}

		/// <summary>
		/// Compares all values in the data record against the values in another datarecord with known true values
		/// </summary>
		/// <param name="dctTrueRecord">The known true record</param>
		public void CompareValuesToTrueRecord(Dictionary<string, string> dctTrueRecord) {
			String strSkippedKeys, strException;

			strException = "";
			strSkippedKeys = "";

			foreach (string strKey in this) {
				if (dctTrueRecord.ContainsKey(strKey) == true) {  //True record has this key, compare it
					if (this.String(strKey).CompareTo(dctTrueRecord[strKey]) != 0) { //Values don't match
						strException = strException + "The values in Key '" + strKey + "' do not match.  Test (local) value: '" + this.String(strKey) + "'  True (external) value: '" + dctTrueRecord[strKey] + "\n";
					}
				} else { //True record does not have this key report it
					strSkippedKeys = strSkippedKeys + strKey + ", ";
				}
			}

			if (strSkippedKeys.CompareTo("") != 0) {  //skipped keys, trim trailing comma
				strSkippedKeys = strSkippedKeys.Substring(0, strSkippedKeys.Length - 2);

				strException = strException + "Skipped comparing some values, True data record (external) does not contain the following keys: " + strSkippedKeys;
			}

			if (strException.CompareTo("") != 0) {  //Found discrepencies, throw an exception
				throw new Exception(strException);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			foreach (string strKey in cdctValues.Keys) {
				yield return strKey;
			}
		}

		/// <summary>
		/// Class used to store the values as well as convert them to the requested types.  Crude impelemntation of variant data type
		/// </summary>
		protected class DataValue {
			private int iNumber;
			private string strString;
			private bool bBoolean;
			private List<string> astrStringList;

			/// <summary>
			/// Constructor that sets a blank value
			/// </summary>
			public DataValue() : this("") { }

			/// <summary>
			/// Constructor that sets a string value
			/// </summary>
			public DataValue(string strText) {
				astrStringList = new List<string>();
				String = strText;
			}

			/// <summary>
			/// Constructor that sets an integer value
			/// </summary>
			public DataValue(int iNum) {
				astrStringList = new List<string>();
				Number = iNum;
			}

			/// <summary>
			/// Constructor that sets a boolen value
			/// </summary>
			public DataValue(bool bFlag) {
				astrStringList = new List<string>();
				Boolean = bFlag;
			}

			/// <summary>
			/// Constructor that sets a string list value
			/// </summary>
			public DataValue(IEnumerable<string> astrStrList) {
				astrStringList = new List<string>();

				foreach (String strValue in astrStringList) {
					astrStringList.Add(strValue);
				}

				StringList = astrStrList;
			}

			/// <summary>
			/// Retrieve the value as a number
			/// </summary>
			public int Number {
				get {
					return iNumber;
				}

				set {
					iNumber = value;
					strString = Convert.ToString(value);

					if (value == 0) {
						bBoolean = false;
					} else {
						bBoolean = true;
					}

					astrStringList.Clear();
				}
			}

			/// <summary>
			/// Retrieve the value as a strting
			/// </summary>
			public string String {
				get {
					return strString;
				}

				set {
					Match rxmResult;

					strString = value;

					try {
						iNumber = Convert.ToInt32(value);
					} catch (Exception) {
						iNumber = 0;
					}

					rxmResult = Regex.Match(value, "^(true|yes|enable|on|1)", RegexOptions.IgnoreCase);
					if ((rxmResult.Success == true) || (iNumber != 0)) {
						bBoolean = true;
					} else {
						bBoolean = false;
					}

					astrStringList.Clear();
				}
			}

			/// <summary>
			/// Retrieve the value as a boolean
			/// </summary>
			public bool Boolean {
				get {
					return bBoolean;
				}

				set {
					bBoolean = value;

					if (value == true) {
						strString = "True";
						iNumber = 1;
					} else {
						strString = "False";
						iNumber = 0;
					}

					astrStringList.Clear();
				}
			}

			/// <summary>
			/// Retrieve the value as a string array
			/// </summary>
			public IEnumerable<string> StringList {
				get {
					return astrStringList;
				}

				set {
					astrStringList.Clear();

					strString = "";
					foreach (string strItem in value) {
						astrStringList.Add(strItem);
						strString = strString + ", " + strItem;
					}

					if (strString.CompareTo("") != 0) { //no reason to correct empty strings
						strString = strString.Substring(2);
					}

					iNumber = astrStringList.Count;
					bBoolean = false;
				}
			}
		}
	}

	/// <summary>
	/// Class for establishing a connection to a MSSQL database and submitting queries
	/// </summary>
	public class DatabaseConn : IDisposable {
		/// <summary>
		/// Connection to the MS SQL database
		/// </summary>
		protected SqlConnection cdbConn;
		private string cstrConnectString, cstrDBName, cstrUserName, cstrPassword, cstrDatabase;
		private int cTimeoutSecs;
		private bool Disposed;

		/// <summary>
		/// Retrieves the server that the connection is to
		/// </summary>
		public string DBServer {
			get {
				return cstrDBName;
			}
		}

		/// <summary>
		/// Retrieves the user name used to conenct to the server
		/// </summary>
		public string DBUserName {
			get {
				return cstrUserName;
			}
		}

		/// <summary>
		/// Retrieves the password used to conenct to the server
		/// </summary>
		public string DBPassword {
			get {
				return cstrPassword;
			}
		}

		/// <summary>
		/// Retrieves the database the conenction is to
		/// </summary>
		public string DBDatabase {
			get {
				return cstrDatabase;
			}
		}

		/// <summary>
		/// Retrieves the number of seconds the connection will wait before timing out
		/// </summary>
		public int TimeoutSeconds {
			get {
				return cTimeoutSecs;
			}
		}

		/// <summary>
		/// Constructor to prepare the class
		/// </summary>
		public DatabaseConn() {
			cstrConnectString = "";
			cdbConn = null;
			Disposed = true;
		}

		/// <summary>
		/// Establishes the connection to the database server.  Will set a 30 second timeout
		/// </summary>
		/// <param name="strServer">The address or IP of the server</param>
		/// <param name="strUserName">The user name to authenticate with</param>
		/// <param name="strPassword">the password to authenticate with</param>
		/// <param name="strDatabase">The database in the server to connect  to</param>
		public void ConnectToDatabase(string strServer, string strUserName, string strPassword, string strDatabase) {
			ConnectToDatabase(strServer, strUserName, strPassword, strDatabase, 30);
		}

		/// <summary>
		/// Establishes the connection to the database server
		/// </summary>
		/// <param name="strServer">The address or IP of the server</param>
		/// <param name="strUserName">The user name to authenticate with</param>
		/// <param name="strPassword">the password to authenticate with</param>
		/// <param name="strDatabase">The database in the server to connect  to</param>
		/// <param name="TimeoutSecs">Number of seconds to wait before timing out</param>
		public void ConnectToDatabase(string strServer, string strUserName, string strPassword, string strDatabase, int TimeoutSecs) {
			cstrDBName = strServer;
			cstrUserName = strUserName;
			cstrPassword = strPassword;
			cstrDatabase = strDatabase;
			cTimeoutSecs = TimeoutSecs;

			//cstrConnectString = "user id=" + strUserName + "; password=" + strPassword + "; data source=" + strServer + "; database=" + strDatabase + "; connection timeout=30";
			cstrConnectString = "user id=" + strUserName + "; password=" + strPassword + "; data source=" + strServer + "; Initial Catalog=" + strDatabase + "; connection timeout=" + TimeoutSecs;

			CheckConnection();
		}

		/// <summary>
		/// Submits an SQL query that expects a single record as a result, then returns the first record found
		/// </summary>
		/// <param name="strSQL">The select query</param>
		/// <param name="oRecord">This value will store the first record in the resulting dataset</param>
		public void SelectQueryOneRecord(string strSQL, DataRecord oRecord) {
			SqlDataReader odbRead;
			SqlCommand odbCommand;
			int iCtr;
			string strName;

			CheckConnection();

			odbCommand = new SqlCommand(strSQL, cdbConn);
			odbRead = odbCommand.ExecuteReader();

			if (odbRead.HasRows == false) {
				oRecord.AddValue("FoundInDB", false);
				odbRead.Close();
				throw new Exception("The SQL query given retrned 0 rows.  The query was:\n" + strSQL);
			}

			odbRead.Read();
			oRecord.AddValue("FoundInDB", true);
			for (iCtr = 0; iCtr < odbRead.FieldCount; iCtr++) {
				strName = odbRead.GetName(iCtr);

				oRecord.AddValue(strName.Trim(), odbRead[strName].ToString().Trim());
			}

			odbRead.Close();
		}

		/// <summary>
		/// Submits an SQL query that expects multiple records as a result
		/// </summary>
		/// <param name="strSQL">The select query</param>
		/// <returns>Returns an enumerated list of DataRecords, each DataRecord is one row from the results</returns>
		public IEnumerable<DataRecord> SelectQueryMultipleRecords(string strSQL) {
			SqlDataReader odbRead;
			SqlCommand odbCommand;
			int iCtr;
			string strName;
			List<DataRecord> lRecords;
			DataRecord oNewRecord;

			CheckConnection();

			lRecords = new List<DataRecord>();
			odbCommand = new SqlCommand(strSQL, cdbConn);
			odbCommand.CommandTimeout = cTimeoutSecs;
			odbRead = odbCommand.ExecuteReader();

			if (odbRead.HasRows == false) {
				odbRead.Close();
				throw new Exception("The SQL query given retrned 0 rows.  The query was:\n" + strSQL);
			}

			while (odbRead.Read() == true) {
				oNewRecord = new DataRecord();

				oNewRecord.AddValue("FoundInDB", true);
				for (iCtr = 0; iCtr < odbRead.FieldCount; iCtr++) {
					strName = odbRead.GetName(iCtr);

					oNewRecord.AddValue(strName, odbRead[strName].ToString());
				}

				lRecords.Add(oNewRecord);
			}

			odbRead.Close();

			return lRecords;
		}

		/// <summary>
		/// Submits an SQL query that expects a single record as a result, then returns the first record found
		/// </summary>
		/// <param name="strSQL">The select query</param>
		/// <returns>REturns the first row of the record set</returns>
		public DataRecord SelectQueryOneRecord(string strSQL) {
			SqlDataReader odbRead;
			SqlCommand odbCommand;
			DataRecord oRecord;
			int iCtr;
			string strName;

			CheckConnection();

			oRecord = new DataRecord();

			odbCommand = new SqlCommand(strSQL, cdbConn);
			odbCommand.CommandTimeout = cTimeoutSecs;
			odbRead = odbCommand.ExecuteReader();

			if (odbRead.HasRows == false) {
				oRecord.AddValue("FoundInDB", false);
				odbRead.Close();
				throw new Exception("The SQL query given retrned 0 rows.  The query was:\n" + strSQL);
			}

			odbRead.Read();
			oRecord.AddValue("FoundInDB", true);
			for (iCtr = 0; iCtr < odbRead.FieldCount; iCtr++) {
				strName = odbRead.GetName(iCtr);

				oRecord.AddValue(strName, odbRead[strName].ToString());
			}

			odbRead.Close();
			return oRecord;
		}

		/// <summary>
		/// Submits a SQL query that will not return a recordset
		/// </summary>
		/// <param name="strSQL">The query to execute</param>
		/// <returns>The number of rows affected by the query</returns>
		public int NonQueryExecute(string strSQL) {
			int odbRead;
			SqlCommand odbCommand;

			CheckConnection();

			if (string.IsNullOrEmpty(strSQL))
			{
				Console.WriteLine("Empty String.");
				return 1;
			}
			else
			{
				odbCommand = new SqlCommand(strSQL, cdbConn);
				odbCommand.CommandTimeout = cTimeoutSecs;
				try
				{
					odbRead = odbCommand.ExecuteNonQuery();
					return odbRead;
				}
				catch (Exception ex)
				{
					throw new Exception("Execution of script failed: " + strSQL + ". Error message: " + ex);
				}
			}
		}

		/// <summary>
		/// Retrieves a recordset that contains only a single field
		/// </summary>
		/// <param name="strSQL">The select query</param>
		/// <returns>Returns an enumerated list of the field</returns>
		public IEnumerable<string> SelectQueryOneField(string strSQL) {
			List<string> astrValues;
			SqlDataReader odbRead;
			SqlCommand odbCommand;
			string strName;

			CheckConnection();

			astrValues = new List<string>();
			//Read one field from multiple records and build an array to return

			odbCommand = new SqlCommand(strSQL, cdbConn);
			odbCommand.CommandTimeout = cTimeoutSecs;
			odbRead = odbCommand.ExecuteReader();

			if (odbRead.HasRows == false) {
				odbRead.Close();
				throw new Exception("The SQL query given returned 0 rows.  The query was:\n" + strSQL);
			}

			while (odbRead.Read() == true) {
				strName = odbRead.GetName(0);
				astrValues.Add(odbRead[strName].ToString());
			}

			odbRead.Close();
			return astrValues;
		}

		/// <summary>
		/// Implementing IDisposable interface, releases all resources
		/// </summary>
		public void Dispose() {
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Managed and unmanaged object release function
		/// </summary>
		/// <param name="Disposing">True to release managed resources as well, false to only release unmanaged resources</param>
		protected virtual void Dispose(bool Disposing)
		{
			if (Disposed == false)
			{
				if (Disposing == true)
				{//Free managed objects
					try {
						cdbConn.Dispose();
					} catch (Exception) { } //Not sure why this gives exceptions

					cdbConn = null;
				}                

				Disposed = true;
			}
		}

		/// <summary>
		/// Destructor for the class, ensures unmanaged resources are released
		/// </summary>
		~DatabaseConn()
		{
			Dispose (false);
		}

		/// <summary>
		/// Checks if the connectio to the database has been made, if not it will attempt to connect using the
		/// current connection string
		/// </summary>
		private void CheckConnection() {
			if ((cdbConn == null) && (string.IsNullOrWhiteSpace(cstrConnectString) == false)) {
				cdbConn = new SqlConnection(cstrConnectString);
				cdbConn.Open();

				GC.ReRegisterForFinalize(this);
				Disposed = false;
			}
		}
	}

	/// <summary>
	/// An extension of the DataRecord class that includes a SQL database connection.  It contains some basic functions
	/// for retrieving data through a select query.
	/// </summary>
	public class DBRecord : DataRecord, IDisposable {
		private bool Disposed;

		/// <summary>
		/// Connection to the SQL database
		/// </summary>
		protected DatabaseConn cdbConn;
		/// <summary>
		/// Basic query to use when retrieving record values
		/// </summary>
		protected string cstrSQLBase;

		/// <summary>
		/// Constructor to prepare the class
		/// </summary>
		public DBRecord() : this(null) { }

		/// <summary>
		/// Constructor to prepare the class
		/// </summary>
		/// <param name="dbConn">Database connection object for this class to use</param>
		public DBRecord(DatabaseConn dbConn)
			: base() {
			cdbConn = dbConn;
			Disposed = false;
		}

		/// <summary>
		/// Property to use to interact with the database connection directly
		/// </summary>
		public DatabaseConn DBConnection {
			get {
				return cdbConn;
			}

			set {
				cdbConn = value;
			}
		}

		/// <summary>
		/// Fill the record indexes with blank values, essentually creating an empty template
		/// </summary>
		public virtual void GenerateEmptyRecord() {
			cdctValues.Clear();

			AddValue("FoundInDB", false);
			AddValue("Type", "");
		}

		/// <summary>
		/// Retrieves the values of the record
		/// </summary>
		/// <param name="strSQL">The SQL query to find the record to load</param>
		protected void GetRecordValues(string strSQL) {
			GetRecordValues(strSQL, cdbConn);
		}

		/// <summary>
		/// Retrieves the values of the record
		/// </summary>
		/// <param name="strSQL">The SQL query to find the record to load</param>
		/// <param name="dbConn">The connection to the MS SQL database to use</param>
		protected void GetRecordValues(string strSQL, DatabaseConn dbConn) {
			if (dbConn == null) {
				throw new Exception("Unable to submit query, no database connection was provided.");
			}

			try {
				dbConn.SelectQueryOneRecord(strSQL, this);
			} catch (Exception exErr) { //Fill with blank values and rethrow error
				GenerateEmptyRecord();

				//Ignore exceptions from no records returned
				if (exErr.Message.StartsWith("The SQL query given retrned 0 rows") == false) {
					throw new Exception("Error executing SQL statement: " + strSQL + "\n" + exErr.Message);
				}
			}
		}

		/// <summary>
		/// Implementing IDisposable interface, releases all resources
		/// </summary>
		public void Dispose() {
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Managed and unmanaged object release function
		/// </summary>
		/// <param name="Disposing">True to release managed resources as well, false to only release unmanaged resources</param>
		protected virtual void Dispose(bool Disposing)
		{
			if (Disposed == false)
			{
				if (Disposing)
				{//Free managed objects
					cdctValues.Clear();
					cdctValues = null;
				}

				//Free unmanaged objects\
				cdbConn.Dispose();

				Disposed = true;
			}
		}

		/// <summary>
		/// Destructor for the class, ensures unmanaged resources are released
		/// </summary>
		~DBRecord()
		{
			Dispose (false);
		}
	}

	/// <summary>
	/// A class to allow changes to the user account the program is running as.
	/// </summary>
	public class WindowsImpersonation : IDisposable {
		const int LOGON32_PROVIDER_DEFAULT = 0;
		const int LOGON32_LOGON_NETWORK = 3;
		const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
		const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
		const int LOGON32_LOGON_INTERACTIVE = 2;

		private WindowsImpersonationContext coImpersonationContext;
		private IntPtr cpiUserHandle;

		// obtains user token
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		// closes open handes returned by LogonUser
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private extern static bool CloseHandle(IntPtr handle);

		/// <summary>
		/// Switches the user account the program is operating under.
		/// </summary>
		/// <param name="strDomain">Domain of the account to switch to</param>
		/// <param name="strUserName">Username of the account to switch to</param>
		/// <param name="strPassword">Password for the account to switch to</param>
		public WindowsImpersonation(string strDomain, string strUserName, string strPassword) {
			coImpersonationContext = null;
			cpiUserHandle = IntPtr.Zero;

			try {
				// Call LogonUser to get a token for the user
				if (LogonUser(strUserName, strDomain, strPassword, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref cpiUserHandle) == false) {
					throw new Exception("Exception impersonating user, error code: " + Marshal.GetLastWin32Error());
				}

				// Begin impersonating the user
				coImpersonationContext = WindowsIdentity.Impersonate(cpiUserHandle);
			} catch (Exception exErr) {
				throw exErr;
			}
		}

		/// <summary>
		/// Dispose of all unmanaged resources created by the class
		/// </summary>
		public void Dispose() {
			if (coImpersonationContext != null) {
				coImpersonationContext.Undo();
			}

			if (cpiUserHandle != IntPtr.Zero) {
				CloseHandle(cpiUserHandle);
			}

			GC.SuppressFinalize(this); //Tell garbage collection not to bother, we're already cleaned up
		}

		/// <summary>
		/// Finalizer to help the garbage collection clean up this class
		/// </summary>
		~WindowsImpersonation() {
			Dispose();
		}
	}
}

