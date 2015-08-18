#Project values
DEPS = MDLN.MGTools.Console.dll MDLN.MGTools.TextureFont.dll
KEY = 
APP = MG-Test.exe
CCARGS = 
REFS = /reference:MonoGame.Framework.dll

#default values
WINMONO = 0
CC = 
#TARGET = exe 
TARGET = winexe

#determine Operating system to set environment
ifndef OS
	OS = $(shell uname -s)
endif
ifeq ($(OS),Windows_NT)
    ifeq ($(WINMONO),1)
    	ENV = Windows/Mono
		CC = c:\Program files (x86)\Mono-3.2.3\bin\dmcs.bat
		REGASM = 
		AL = c:\Program files (x86)\Mono-3.2.3\bin\al.bat
		#GACUTIL = c:\Program files (x86)\Mono-3.2.3\bin\gacutil.bat
		SN = c:\Program files (x86)\Mono-3.2.3\bin\sn.bat
		DEL = del /Q
    else
    	ENV = Windows/.Net
		CC = C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
		#REGASM = C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe		
		AL = C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\al.exe
		#GACUTIL = C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\gacutil.exe
		SN = C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe
		DEL = del /Q
    endif
endif
ifeq ($(OS),Linux)
	ENV = Linux/Mono
	CC = dmcs
	REGASM = 
	AL = al
	#GACUTIL = gacutil
	SN = sn
	DEL = rm -f
endif
ifeq ($(OS),Darwin)
	ENV = OS X/Mono
	CC = dmcs
	REGASM = 
	AL = al
	#GACUTIL = gacutil
	SN = sn
	DEL = rm -f
endif

main: setenv clean $(KEY) $(DEPS) $(APP)
	@ echo "----------------------------------------------------------"
	@ echo "Compiled All Components"

setenv :
	@ echo "----------------------------------------------------------"
	@ echo "Using Environment: $(ENV)"
	@ echo ""
    

%.exe : %.cs
	@ echo "----------------------------------------------------------"
	@ echo "Compiling $@"
	$(CC) $(CCARGS) $(REFS) /target:$(TARGET) /out:$@ $^
#	$@
	@ echo ""

%.dll : %.cs
	@ echo "----------------------------------------------------------"
    ifneq ($(GACUTIL),)
		@ echo "Removing $@ from the GAC"
		$(GACUTIL) /u $@
    endif
	@ echo "Compiling $@"
    ifeq ($(KEY),)
		$(CC) $(CCARGS) $(REFS) /target:library /out:$@ $^
    else
		$(CC) $(CCARGS) $(REFS) /target:library /out:$@ $^ /keyfile:$(KEY)
    endif
    ifneq ($(GACUTIL),)
		@ echo "Adding $@ to the GAC"
		$(GACUTIL) /i $@
    endif
    ifneq ($(REGASM),)
		@ echo "Registering $@ for COM"
		$(REGASM) /codebase $@
    endif
	$(eval REFS = $(REFS) /reference:$@)
	@ echo ""

%.snk : 
	@ echo "----------------------------------------------------------"
	@ echo "Creating keyfile $@ for strong named libraries"
	$(SN) -k $(KEY)
	@ echo ""
	
install : 
	@ echo "----------------------------------------------------------"
	@ echo "Installing compiled libraries and executables"
	@ echo ""
	
setup :
	@ echo "----------------------------------------------------------"
	@ echo "Preparing environment, registering DLL's and installing dependencies
	@ echo ""
	
clean :
	@ echo "----------------------------------------------------------"
	@ echo "Cleaning up environment"
    ifdef DEPS
		$(DEL) $(DEPS)
    endif
	$(DEL) $(APP)
	@ echo ""
