import os
import re

directory = r'c:\Nabil\Projects\Farming Sim 3D\Astro-Pioneer\Assets\Scripts'

for root, _, files in os.walk(directory):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Check if it has a singleton
            if 'public static ' in content and ' Instance ' in content:
                modified = False
                
                # Check if it has Instance = null;
                if 'Instance = null;' not in content and 'Instance=null;' not in content:
                    # Does it have OnDestroy?
                    if 'void OnDestroy()' in content:
                        content = re.sub(r'void OnDestroy\(\)\s*\{', 'void OnDestroy()\n        {\n            if (Instance == this) Instance = null;', content)
                        modified = True
                    else:
                        # Append after Awake
                        content = re.sub(r'(void Awake\(\)[\s\S]*?\n\s*\})', r'\1\n\n        void OnDestroy()\n        {\n            if (Instance == this) Instance = null;\n        }', content)
                        modified = True
                
                if modified:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Patched: {file}")
