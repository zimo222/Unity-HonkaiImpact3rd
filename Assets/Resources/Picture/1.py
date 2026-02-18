import os
import fnmatch

folder_path = "."  # 当前文件夹，也可以改成具体路径如 "D:/你的文件夹"
prefix = "Icon_"

for filename in fnmatch.filter(os.listdir(folder_path), "Icon_*"):
    if filename.startswith(prefix):
        new_name = filename[len(prefix):]  # 去掉前缀
        old_path = os.path.join(folder_path, filename)
        new_path = os.path.join(folder_path, new_name)
        os.rename(old_path, new_path)
        print(f"已重命名: {filename} -> {new_name}")