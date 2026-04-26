import json

with open('all_issues_utf8.json', 'r', encoding='utf-8-sig') as f:
    issues = json.load(f)

md = '# Astro-Pioneer Issue Audit & Testing Plan\n\n'
md += '## 🚨 High Priority Regression (Refactor Impact)\n'
md += 'These CLOSED issues need to be tested immediately because our recent changes to `ServiceLocator`, `ChunkManager`, and `ChunkRenderer` might have broken them.\n\n'

for i in issues:
    if i['state'] == 'CLOSED':
        md += f"- [ ] **#{i['number']}** - {i['title']}\n"

md += '\n## 🏗️ Core Open Mechanics (Needs Implementation/Testing on New Architecture)\n'
for i in issues:
    if i['state'] == 'OPEN' and ('Developer' in i['title'] or 'Mechanic' in i['title'] or 'System' in i['title'] or 'QA' in i['title']):
        md += f"- [ ] **#{i['number']}** - {i['title']}\n"

md += '\n## 🎨 Assets, UI, & Audio (Open)\n'
for i in issues:
    if i['state'] == 'OPEN' and ('Artist' in i['title'] or 'UI' in i['title']):
        md += f"- [ ] **#{i['number']}** - {i['title']}\n"

with open('full_testing_audit.md', 'w', encoding='utf-8') as f:
    f.write(md)
