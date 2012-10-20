Seringa
=======

Seringa - SQL Injection framework

Seringa(Romanian for seringe) is an SQL injection framework featuring high customizability and a user-friendly interface.
It is completely open source. Uses the .NET 4.0 framework and Windows Presentation Foundation(WPF) for the GUI.
With regard to design it utilizes the Strategy Pattern to distinguish between various SQLi strategies whilst storing
other relevant data such as exploits, payloads and patterns in xml files so that the framework can be easily customized
from the outside(a manifestation of the Open-Closed Principle).
Seringa allows you to:
- scan Google search results given a search string
- test search results for SQLi vulnerability
- test a single url for vulnerability
- extract a database structure(databases,tables,columns) in a tree form
- execute given payloads and receive results(some predefined queries include current database name, current database user,
  current database version etc)
- save your penetration testing process to a file(mapping file) and load it later
- use a proxy(regular or socks) when testing

Concepts:
Injection Strategies
- ways of actually running a SQL injection
- require their own distinct classes in the code
- not modifiable without recompiling
- the 2 available types at the time of writing are "UNION Based"(referring to the use of the UNION SQL command) 
  and "ERROR Based"(referring to errors being spilled out by the web application)
- Injection Strategy classes are required to implement the IInjectionStrategy interface
DBMSs
- short for Database Management System
- refer to the underlying DBMS that the web application sends commands to
- the DBMS values seen in the GUI are extracted from the exploits file(see the Exploits concept)
Exploits
- the actual SQL commands that cause a vulnerable system to do what the penetration tester wants
- they are configurable in the exploits.xml file that is found in the xml folder each <exploit> node in sed file represents
  a single exploit
- each exploit works for a particular DBMS as specified by the dbms attribute of the <exploit> node
Payloads
- what the penetration tester wants to do to the system
- configurable in the payloads.xml file
- also dependable on the DBMS
Patterns
- used when testing if a particular url is SQL injectable
- each pattern is a message that the targeted web application might output when it's tested if it is vulnerable to SQLi attacks
- configurable in patterns.xml
Ipcheckers
- when using a proxy with Seringa you might want to check what your ip actually is from within the application
- this can easily be done using a free ip checker site
- the ipcheckers.xml file allows for the free site to be configured to your favourite ip checker site
