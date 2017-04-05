EditableDataGrid
==========
EditableDataGrid is a custom datagrid with the ability to use editable datagrid column types. The implementation is not perfect and I consider it a work in progress. 
The lack of the ability to override WProc in Compact Framework requires some work arrounds to beable to customize the behavior of key events on certian controls. 
EditableDataGridCF makes use of the `KeyPressDispatcher` class and `IKeyPressProcssor` interface to workaround some of the finiky key press behavior of controls in Compact Framework.
The purpose of the `KeyPressDispatcher` class is to work with your `Form` to find and notify any focused control that implements `IKeyPressProcssor` of any key event; 
Taking advantage of the fact when a `Form` class had `KeyPreview = true` it is able to intercept key presses that may normaly be swallowed by the control.

Besure to check the issues for this project as there are some.



