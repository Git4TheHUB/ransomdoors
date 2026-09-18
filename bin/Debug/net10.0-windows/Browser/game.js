const game = document.querySelector('#game');
const warning = document.querySelector('#warning');
const attack = document.querySelector('#attack');
const download = document.querySelector('#download');
const ransom = document.querySelector('#ransom');
const amount = document.querySelector('#amount');
const timer = document.querySelector('#timer');
const drawer = document.querySelector('#drawer');
const dropzone = document.querySelector('#dropzone');
const spawn = document.querySelector('#spawn');
const desktopShell = document.querySelector('#desktopShell');
const explorerFiles = document.querySelector('#explorerFiles');
const explorerStatus = document.querySelector('#explorerStatus');
const transferCoinsButton = document.querySelector('#transferCoins');
const desktopClock = document.querySelector('#desktopClock');
let ransomLeft = 0;
let ransomTime = 0;
let active = false;
let mouseMoved = false;
let usedCoins = new Set();
let tauntWindows = [];
let mouseListener;
let selectedCoins = new Set();
let coinEntries = [];

const tauntImages = ['glitch1.jpg', 'glitch2.jpeg', 'glitch3.jpg', 'glitch4.jpg', 'glitch5.jpg', 'idiot.png', 'tauntface.png', 'tauntflower.png'];
const tauntTitles = ['RANS0M', 'MOSNAR', 'RANSOM', 'M0NARS', 'YOU ARE AN IDIOT', 'Untitled', 'I FOUND YOU', 'RANSOM.exe'];

function sleep(ms) { return new Promise(resolve => setTimeout(resolve, ms)); }
function showOnly(section) {
  [warning, attack, download].forEach(item => item.style.display = item === section ? '' : 'none');
  desktopShell.classList.toggle('open', section === ransom);
  ransom.style.display = section === ransom ? '' : 'none';
}
function randomItem(items) { return items[Math.floor(Math.random() * items.length)]; }
function openTaunt() {
  const popup = tauntWindows.shift() || window.open('taunt.html', `taunt-${Date.now()}`, 'popup,width=350,height=350,resizable=no');
  if (popup) popup.addEventListener('load', () => popup.postMessage({ image: randomItem(tauntImages), title: randomItem(tauntTitles) }, '*'));
}
function reserveTauntWindows() {
  for (let index = 0; index < 9; index++) {
    const popup = window.open('taunt.html', `taunt-${Date.now()}-${index}`, 'popup,width=350,height=350,resizable=no');
    if (popup) tauntWindows.push(popup);
  }
}
function updateStats() {
  amount.textContent = String(Math.max(0, ransomLeft));
  const seconds = Math.max(0, ransomTime);
  timer.textContent = `TIME: ${String(Math.floor(seconds / 60)).padStart(2, '0')}:${String(seconds % 60).padStart(2, '0')}`;
}
function createCoin(value) {
  const coin = document.createElement('span');
  coin.className = 'coin';
  coin.draggable = true;
  coin.textContent = `${value} GOLD`;
  coin.dataset.value = value;
  coin.dataset.id = crypto.randomUUID();
  coin.addEventListener('dragstart', event => event.dataTransfer.setData('text/plain', coin.dataset.id));
  drawer.append(coin);
  const entry = document.createElement('button');
  entry.type = 'button';
  entry.className = 'explorer-file';
  entry.dataset.id = coin.dataset.id;
  entry.innerHTML = `<img src="../Assets/Gold.png" alt=""> <span>coin${value}.gold</span><small>${value} GOLD</small>`;
  entry.addEventListener('click', () => {
    entry.classList.toggle('selected');
    if (entry.classList.contains('selected')) selectedCoins.add(entry.dataset.id);
    else selectedCoins.delete(entry.dataset.id);
    explorerStatus.textContent = `${selectedCoins.size} coin${selectedCoins.size === 1 ? '' : 's'} selected`;
  });
  explorerFiles.append(entry);
  coinEntries.push({ entry, coin });
}
function transferCoins() {
  coinEntries.filter(item => selectedCoins.has(item.coin.dataset.id)).forEach(item => {
    if (usedCoins.has(item.coin.dataset.id)) return;
    usedCoins.add(item.coin.dataset.id);
    ransomLeft -= Number(item.coin.dataset.value);
    item.entry.remove();
    item.coin.remove();
  });
  selectedCoins.clear();
  explorerStatus.textContent = 'Coins transferred to ransom.exe';
  updateStats();
  if (ransomLeft <= 0) window.location.href = 'thank-you.html';
}
function makeWindowInteractive(windowElement) {
  const bar = windowElement.querySelector('.window-bar');
  let drag = null;
  bar.addEventListener('pointerdown', event => {
    if (event.target.closest('button')) return;
    windowElement.style.zIndex = String(Date.now());
    const rect = windowElement.getBoundingClientRect();
    drag = { x: event.clientX, y: event.clientY, left: rect.left, top: rect.top };
    bar.setPointerCapture(event.pointerId);
  });
  bar.addEventListener('pointermove', event => {
    if (!drag) return;
    windowElement.style.left = `${Math.max(0, drag.left + event.clientX - drag.x)}px`;
    windowElement.style.top = `${Math.max(28, drag.top + event.clientY - drag.y)}px`;
  });
  bar.addEventListener('pointerup', () => { drag = null; });
}
function downloadCoins() {
  [25, 50, 100, 250, 500].forEach((value, index) => {
    const data = JSON.stringify({ RANSOM_COIN: crypto.randomUUID(), value });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(new Blob([data], { type: 'application/octet-stream' }));
    link.download = `coin${index + 1}.gold${index + 1}`;
    link.click();
    URL.revokeObjectURL(link.href);
  });
}
async function startRansom() {
  if (active) return;
  active = true;
  spawn.disabled = true;
  showOnly(warning);
  mouseMoved = false;
  mouseListener = () => { mouseMoved = true; };
  window.addEventListener('mousemove', mouseListener, { once: false });
  await sleep(1000);
  window.removeEventListener('mousemove', mouseListener);
  if (!mouseMoved) { active = false; spawn.disabled = false; return; }
  showOnly(attack);
  await sleep(850);
  showOnly(download);
  await sleep(1300);
  game.classList.add('infected');
  ransomLeft = 500;
  ransomTime = 90;
  updateStats();
  showOnly(ransom);
  desktopClock.textContent = '00:00';
  for (let i = 0; i < 9; i++) openTaunt();
  const interval = setInterval(() => { ransomTime--; updateStats(); if (ransomTime <= 0 || ransomLeft <= 0) clearInterval(interval); }, 1000);
  while (ransomTime > 0 && ransomLeft > 0) await sleep(100);
  if (ransomLeft <= 0) window.location.href = 'thank-you.html';
  else { active = false; spawn.disabled = false; showOnly(warning); game.classList.remove('infected'); }
}
function acceptCoin(file) {
  if (!file.name.match(/^\.?(gold\d|crucifix)$/i) && !file.name.match(/\.gold\d$/i)) return;
  const reader = new FileReader();
  reader.onload = () => {
    try {
      const coin = JSON.parse(reader.result);
      if (usedCoins.has(coin.RANSOM_COIN)) return;
      usedCoins.add(coin.RANSOM_COIN);
      ransomLeft -= file.name.endsWith('.crucifix') ? 500 : Number(coin.value || 0);
      updateStats();
      if (ransomLeft <= 0) window.location.href = file.name.endsWith('.crucifix') ? 'crucifix.html' : 'thank-you.html';
    } catch { }
  };
  reader.readAsText(file);
}
dropzone.addEventListener('dragover', event => { event.preventDefault(); dropzone.classList.add('over'); });
dropzone.addEventListener('dragleave', () => dropzone.classList.remove('over'));
dropzone.addEventListener('drop', event => { event.preventDefault(); dropzone.classList.remove('over'); [...event.dataTransfer.files].forEach(acceptCoin); });
spawn.addEventListener('click', startRansom);
spawn.addEventListener('click', reserveTauntWindows, { once: true });
document.querySelector('#downloadCoins').addEventListener('click', downloadCoins);
transferCoinsButton.addEventListener('click', transferCoins);
for (const value of [25, 50, 100, 250, 500]) createCoin(value);
document.querySelectorAll('.fake-window').forEach(makeWindowInteractive);
