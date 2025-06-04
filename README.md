# BlockyDash

## Setting up LFS
### **Setting Up Git LFS**
1. **`git lfs install`** – Installs and initializes Git LFS, ensuring it's properly set up in the repository.
2. **`git lfs track "<filetype>"`** – Specifies which file types (e.g., `.unity`, `.mp4`, `.psd`, `.fbx`) should be tracked using Git LFS.

### **Managing Git Attributes**
3. **`git add .gitattributes`** – Adds the `.gitattributes` file to track changes related to LFS settings.
4. **`git commit -m "<message>"`** – Commits the `.gitattributes` file changes with a message.

### **Pushing Files Using Git LFS**
5. **`git push origin <branch>`** – Pushes the repository to GitHub, ensuring LFS-tracked files are properly uploaded.

### **Migrating Existing Files to Git LFS**
6. **`git lfs migrate import --include="<filetypes>"`** – Converts existing files to be tracked by Git LFS (useful if large files were already committed).
7. **`git push origin <branch> --force`** – Force-pushes changes after applying LFS tracking to existing files.

The video walks through setting up Git LFS, tracking specific file types, and ensuring that large files can be uploaded without GitHub rejecting them. Let me know if you need further clarification on any step! 🚀
