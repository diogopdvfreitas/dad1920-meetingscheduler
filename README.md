# MSDAD 2019/20

### Group 9

Diogo Freitas - 81586  
Mara Caldeira - 83506  
Rita Prates - 86507  

***

### Build & Run

Go to file /Server/App.config and all the servers you intend to have on your system.<br/>
Servers should be added to the config file in the following format:

&nbsp;&nbsp;&nbsp;&nbsp;\<configuration\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\<appSettings\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\<add key="tcp://localhost:3001/server1"\/\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\<\/appSettings\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;\<\/configuration\><br/>

Where key is your server URL.

Go to file /PuppetMaster/App.config and all the PCS' you intend to have on your system.<br/>
PCS' should be added to the config file in the following format:

&nbsp;&nbsp;&nbsp;&nbsp;\<configuration\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\<appSettings\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\<add key = "PCS1" value = "tcp://localhost:10000/PCSSERVICE"\/\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\<\/appSettings\><br/>
&nbsp;&nbsp;&nbsp;&nbsp;\<\/configuration\><br/>

Where key is your PCS identifier and value your PCS URL.  
NOTE: Your PCS' should all be exposed on port 10000.

On the project root folder, a start.bat windows batch file has all the needed configuration to run the project.  
If any changes are made to the project source code, including those to the config files suggested in this README, build the code before executing the start.bat file.