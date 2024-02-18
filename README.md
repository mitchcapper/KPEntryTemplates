KP Entry Templates for KeePass 2.x
=================================
## Warning: KeePass 2.39 significantly changed the password control. Version 7.8 and higher are only compatible with keepass 2.39 and higher.

CHANGES
-----
- 8.0 High DPI support completely redone, complete re-work of KeePass resize support properly handle the UI. Multi-line textboxes also improved.
- 7.8 KeePass 2.39> support, better password confirmation support, breaks keepass support < 2.39. Rich Textbox support.
- 7.7 KeePassResize support thanks to @h-a-s-h
- 7.6 ComboBox width fixed to full size (rather than smallest entry)
- 7.5 Compatibility with KeePass 2.36 and better
- 7.3 Added inline url option that has a clickable link to open the url
- 7.2 Fixed bug that prevented working on *nix platforms, thanks to @x09
- 7.01 Fixed bug causing crash if first field was not a textbox on a template
- 7.0 Moved to git(hub), KP 2.3 support, High DPI support
- 6.0 Bugfixes, listbox/combobox support, specify number of lines for inline boxes, KP 2.18 support

![KPEntryTemplates Screenshot](https://raw.githubusercontent.com/mitchcapper/KPEntryTemplates/master/screenshot.png "KPEntryTemplates Screenshot")

OVERVIEW
-----
KPEntryTemplates (KPET) is a plug-in that is supposed to make it easier for 
KeePass to be used for storing additional things than web logins and extend 
the built in templating system by providing a UI. 

Bug reports and feature requests should be filed in GitHub while general discussion in https://gitter.im/KeePassUnofficial/Plugins 

The primary interface for the plug-in is an 
extra tab that shows up in the main Add Entry form.   By default going to 
the tab on a new entry will present you with one of two buttons, 
"Init As Template" or "Set Template Parent".  "Init As Template" shows up 
if it is in the Template Group for the database.

"Init As Template" marks the entry as a template, and shows the GUI builder 
interface on the tab.   This is a table that has 4 columns, "Title", 
"Field", "Field Name", and "Type".  "Title" is the title that will be 
displayed to the user to explain the field.  "Field" is either one of the 
default fields for the entry (Title, Username, Password, Expiry Date, etc.) 
or Custom for a standard additional string field.  The "Field Name" is the 
actual name for the field, this must be unique (just like string fields) 
and is the actual field name the field is stored under.    Finally, the 
"Type" is how the field is displayed to the user, it has many options, most 
of which are self explanatory.  A few may require some explanation:

- "Inline" is a standard textbox
- "Rich Textbox" is a rich textbox (URL highlighting and such like the normal notes box).
- "Popout" shows the standard custom field editing window

- "Divider" is not a field at all, but rather can be used to section the 
GUI to the user

If the type starts with the word "Protected", that means it is in memory 
encrypted.  Note that "Field Name" and "Type" cannot be changed, except on 
custom entries.   Rows can be deleted by selecting the row header (box on 
the left) and clicking Delete.  Rows can be re-ordered by dragging the row 
by the header.

Once you have a template setup, you can now create a child.  Create an 
entry anywhere other than your Template folder and when you go to the 
Template tab you will see "Set Template Parent". Click this and choose the 
template you just created.  Once you do this you will see the GUI you built 
show up.  It will now show this GUI by default when you open an entry that 
you set the parent to.  Editing the fields in the GUI has the same effect 
as editing them by hand, so you do not have to use the GUI.

ADDITIONAL FEATURES OF THE GUI
-----
- If you add the "Confirm Field" to your GUI it will show the password 
generator option and ensure the confirmed password matches the password 
field before allowing the user to save (same as the main entry editing). 
Note you should always have the "Password Field" in your template if you 
have the "Confirm Field", otherwise, the Confirm is somewhat useless.

- When you click the dropdown in the main menu to create a new template 
based entry, if that template is a GUI template it will automatically strip 
all the GUI template strings from it and make the new entry a proper child 
of it.

- Some types (inline fields and listboxes) may have options, click the
option button on the GUI builder to set the options for the item.

- Two options have been added to the context right click) menu for entries. 
For all entries (including multiple entries at once) you can click 
"Set Template Parent", to assign a template as their parent.  Secondly, on 
entries that already have a template parent, there is a 
"Copy Template String" sub menu that allows you to copy any of the strings 
to the clipboard, this is very similar to the "Copy Custom String" sub 
menu, but lets you do it by the template title rather than whatever the 
fieldname is.

IMPORTANT NOTES
-----
The plug-in relies on you having set a Template Group for the database, so 
make sure you do this and store your templates in this folder.

The plug-in stores all template GUI setup in the template entry itself (in 
the extra fields) these can be removed by clicking "Remove As Template". 
The only other change the plug-in makes is on entries that are a child of a 
template they have 1 extra field set (Removable by clicking Remove As 
Template Child) There is _no_ outside data or configuration stored. Meaning 
you do not have to copy settings or additional files, if you copy your 
keepass database to another computer and have the plug-in installed it will 
just work as it did on your last computer.


DEFAULT VALUES  
-----  
In order to save default values inside template one should know a small trick: the name of your field on `Template` tab should be the same with the name of string field ("Field Name" on the main GUI). I.e. every field to be defaulted should have correspondent field on `Advanced` tab populated with some value to give it a default. 

INSTALLATION
-----
- Download from https://github.com/mitchcapper/KPEntryTemplates/releases
- Extract the plug-in (KPEntryTemplates.plgx) and place in the KeePass 
program directory

- Start KeePass and open a database

- In KeePass, click "Edit > Add Group" and name it something like 
"Templates", press Enter to finish adding the new group.

- Select "File > Database Settings..." and select the "Advanced" tab.

- In the "Templates" group set, set the "Entry templates group:" select the 
group you just created, and click "OK".  This will close the database 
settings dialog and return you to the main application.

- In the main application, select your "Templates" folder and click 
"Edit > Add Entry..."

- On the "Add Entry" dialog, select the "Template" tab and click 
"Init As Template"

- Build you new template by adding whatever fields you desire

- Be sure to switch back to the "Entry" tab and enter the name of the 
template in the "Title" field.

- Once you are satisfied, click "OK" to save the template

- Your template is now ready to use when adding or editing entries

Examples
-----

You can find two sample database files in the Samples folder click view file then the download icon.  Each database has a blank password (check the password box but leave it empty).  `StarterWithTemplates.kdbx` is like the default Starter DB but includes some template example entries and template parents.  The `Templates.kdbx` has just the template parents.  You can import it into your existing file or use it as a starter database.  These samples were created by @IncPlusPlus thank you!