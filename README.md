# vb6-merge-driver
git merge driver for Visual Basic 6 files (frm ctl vbp)

## Requirements
`git.exe` needs to be available on your PATH

## How to install:
Edit your `.gitconfig` and `.gitattributes`, ie:

```
#.gonfig
[merge "vbpSmart"]
	name = vb6-merge-driver
	driver = <path>/<to>/vb6-merge-driver.exe %O %A %B %L %P
	recursive = binary
```

```
#.gittributes
*		text=auto
*.vbp		text	eol=crlf	merge=vbpSmart
*.frm		text	eol=crlf	merge=vbpSmart
*.bas		text	eol=crlf	
*.cls		text	eol=crlf
*.ctl		text	eol=crlf	merge=vbpSmart
*.frx		-text 
*.ctx		-text
*.res		-text
```


## What does this driver do?

### Reference & Object
* Ignores GUID and Library Names are ignored.
* Matches version number and library path
* Relative Library paths and resolved their 
* If two references match, they're merged with a keep-ours strategy

this allows to solve merging conflicts like

```
<<<<< OUR:A\D\E.vbp
Reference=*\G{1C61C9A6-E720-4741-B8DA-827B9CEBFBD1}#1.0#0#..\..\A\B\C.dll# 
=====
Reference=*\G{2555402F-0C65-4FE8-90E0-7757F47E3275}#1.0#0#..\B\C.dll#
>>>>> THEIR:A\D\E.vbp
```

### Module, Class, Form & UserControl:
* Keep the newest line

### Everything else:
* Quotes are ignored when matching values, ie, the following conflict would resolve to **THEIR**.


```
<<<<<< OUR:foo.vbp
CodeViewDebugInfo="0"
|||||| BASE:foo.vbp
CodeViewDebugInfo=0
======
CodeViewDebugInfo=1
>>>>> THEIR:foo.vbp
```

## How are `.ctl` and `.frm` files handled ?
Only the preamble is merged, after that the merging is finished calling `git merge-file`
