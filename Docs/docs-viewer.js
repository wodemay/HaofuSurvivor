(function () {
  const docs = Array.isArray(window.DOCS_DATA) ? window.DOCS_DATA : [];
  const nav = document.getElementById('nav');
  const content = document.getElementById('content');
  const search = document.getElementById('search');
  const esc = s => s.replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  function inline(s) { return esc(s).replace(/`([^`]+)`/g, '<code>$1</code>').replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>').replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2">$1</a>'); }
  function render(md) {
    const lines = md.replace(/\r/g, '').split('\n'), out = []; let list = null, table = false, code = false;
    const closeList = () => { if (list) { out.push('</' + list + '>'); list = null; } };
    for (const line of lines) {
      if (line.trim().startsWith('```')) { closeList(); if (!code) { out.push('<pre><code>'); code = true; } else { out.push('</code></pre>'); code = false; } continue; }
      if (code) { out.push(esc(line) + '\n'); continue; }
      if (!line.trim()) { closeList(); table = false; continue; }
      let m = line.match(/^(#{1,4})\s+(.+)$/); if (m) { closeList(); out.push('<h' + m[1].length + '>' + inline(m[2]) + '</h' + m[1].length + '>'); continue; }
      if (/^\s*[-*]\s+/.test(line)) { if (list !== 'ul') { closeList(); out.push('<ul>'); list = 'ul'; } out.push('<li>' + inline(line.replace(/^\s*[-*]\s+/, '')) + '</li>'); continue; }
      if (/^\s*\d+\.\s+/.test(line)) { if (list !== 'ol') { closeList(); out.push('<ol>'); list = 'ol'; } out.push('<li>' + inline(line.replace(/^\s*\d+\.\s+/, '')) + '</li>'); continue; }
      if (/^\s*>/.test(line)) { closeList(); out.push('<blockquote>' + inline(line.replace(/^\s*>\s?/, '')) + '</blockquote>'); continue; }
      if (/^\|.*\|$/.test(line)) { closeList(); const cells = line.trim().slice(1,-1).split('|').map(x => x.trim()); if (/^[-: ]+$/.test(cells[0] || '')) continue; const tag = table ? 'td' : 'th'; if (!table) { out.push('<table><thead><tr>'); cells.forEach(c => out.push('<th>' + inline(c) + '</th>')); out.push('</tr></thead><tbody>'); table = true; } else { out.push('<tr>'); cells.forEach(c => out.push('<' + tag + '>' + inline(c) + '</' + tag + '>')); out.push('</tr>'); } continue; }
      closeList(); out.push('<p>' + inline(line) + '</p>');
    }
    closeList(); if (table) out.push('</tbody></table>'); if (code) out.push('</code></pre>'); return out.join('');
  }
  function draw(filter) { nav.innerHTML = ''; docs.filter(d => !filter || (d.title + d.path).toLowerCase().includes(filter.toLowerCase())).forEach((d, i) => { const b = document.createElement('button'); b.textContent = d.title; b.dataset.path = d.path; b.onclick = () => open(d.path); nav.appendChild(b); }); }
  async function open(path) {
    const d = docs.find(x => x.path === path) || docs[0];
    if (!d) { content.innerHTML = '<p class="empty">没有找到文档。</p>'; return; }
    document.querySelectorAll('nav button').forEach(b => b.classList.toggle('active', b.dataset.path === d.path));
    history.replaceState(null, '', '#' + encodeURIComponent(d.path));
    content.innerHTML = '<p class="empty">正在加载…</p>';
    if (!d.content) {
      window.DOC_CONTENT = '';
      try {
        await new Promise((resolve, reject) => { const script = document.createElement('script'); script.src = d.file; script.onload = resolve; script.onerror = reject; document.head.appendChild(script); });
      } catch (_) {
        content.innerHTML = '<p class="empty">文档加载失败：' + esc(d.path) + '</p>';
        return;
      }
      d.content = window.DOC_CONTENT || '';
    }
    content.innerHTML = '<div class="meta">' + esc(d.path) + '</div>' + render(d.content);
  }
  search.oninput = () => draw(search.value); draw(''); open(decodeURIComponent(location.hash.slice(1)) || (docs[0] && docs[0].path));
})();
