
ODIR=ship
SRCDIR=..

CS_FLAGS=/debug:full

!ifdef DEBUG
ODIR=debug
CS_FLAGS=$(CS_FLAGS) /define:DEBUG /debug:full /debug+
!endif

target: chdir SList.exe

clean: 
	-del /q $(ODIR)\*.*
	
chdir:
	@-mkdir $(ODIR) > NUL 2>&1
	@cd $(ODIR)  
	@echo Changed directory to $(ODIR)...

AssemblyInfo.netmodule: ..\AssemblyInfo.cs
	csc $(CS_FLAGS) /target:module /out:AssemblyInfo.netmodule ..\AssemblyInfo.cs

SList.exe: AssemblyInfo.netmodule $(SRCDIR)\SList.cs $(SRCDIR)\App.ico
	csc $(CS_FLAGS) /target:winexe /out:SList.exe /addmodule:AssemblyInfo.netmodule $(SRCDIR)\SList.cs
	

        
