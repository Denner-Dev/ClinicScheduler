const elData = document.getElementById("data");
const btnBuscar = document.getElementById("btnBuscar");
const statusEl = document.getElementById("status");

const diaInfoEl = document.getElementById("diaInfo");
const horariosEl = document.getElementById("horarios");
const slotsVazioEl = document.getElementById("slotsVazio");

const elNome = document.getElementById("nome");
const elEmail = document.getElementById("email");
const elSelecionado = document.getElementById("selecionado");
const btnAgendar = document.getElementById("btnAgendar");
const confirmacaoEl = document.getElementById("confirmacao");

const btnAtualizar = document.getElementById("btnAtualizar");
const listaEl = document.getElementById("lista");
const listaVaziaEl = document.getElementById("listaVazia");

let slotSelecionado = null;

const MIN_DATE = "2026-01-01";
const MAX_DATE = "2035-12-31";

function showAlert(el, type, msg) {
  if (!msg) {
    el.hidden = true;
    el.textContent = "";
    el.className = "alert";
    return;
  }
  el.hidden = false;
  el.textContent = msg;
  el.className = `alert ${type || ""}`.trim();
}

function limparHorarios() {
  horariosEl.innerHTML = "";
  slotSelecionado = null;
  elSelecionado.value = "";
  btnAgendar.disabled = true;
  slotsVazioEl.hidden = false;
}

function pad2(n) { return String(n).padStart(2, "0"); }

function formatarDataHora(iso) {
  const dt = new Date(iso);
  return `${pad2(dt.getDate())}/${pad2(dt.getMonth() + 1)}/${dt.getFullYear()} ${pad2(dt.getHours())}:${pad2(dt.getMinutes())}`;
}

function nomeDiaSemana(dateStr) {
  const d = new Date(`${dateStr}T00:00:00`);
  const dias = ["domingo","segunda-feira","terça-feira","quarta-feira","quinta-feira","sexta-feira","sábado"];
  return dias[d.getDay()];
}

function ehFimDeSemana(dateStr) {
  const d = new Date(`${dateStr}T00:00:00`);
  const day = d.getDay();
  return day === 0 || day === 6;
}

function dentroDoIntervalo(dateStr) {
  // dateStr: YYYY-MM-DD (comparação lexical funciona)
  return dateStr >= MIN_DATE && dateStr <= MAX_DATE;
}

async function readErrorText(res) {
  const txt = (await res.text()).trim();
  if (!txt) return `Erro HTTP ${res.status}`;

  // Se vier HTML, mostra genérico
  if (txt.startsWith("<!doctype") || txt.startsWith("<html")) {
    return "Erro no servidor. Tente novamente.";
  }

  return txt;
}

async function buscarHorarios() {
  // NÃO limpamos confirmacaoEl aqui, para não apagar a mensagem de sucesso após agendar
  showAlert(statusEl, "", "");
  limparHorarios();

  const data = elData.value; // YYYY-MM-DD
  if (!data) {
    showAlert(statusEl, "info", "Selecione uma data para buscar horários.");
    return;
  }

  diaInfoEl.textContent = `Dia: ${nomeDiaSemana(data)}`;

  // 1) valida intervalo primeiro
  if (!dentroDoIntervalo(data)) {
    showAlert(statusEl, "err", `Selecione uma data entre 01/01/2026 e 31/12/2035.`);
    return;
  }

  // 2) fim de semana (mensagem rápida)
  if (ehFimDeSemana(data)) {
    showAlert(statusEl, "info", "Fim de semana: não há horários disponíveis. Escolha uma data de segunda a sexta.");
    return;
  }

  btnBuscar.disabled = true;
  showAlert(statusEl, "info", "Buscando horários disponíveis...");

  try {
    const res = await fetch(`/available?date=${encodeURIComponent(data)}`);
    if (!res.ok) {
      const msg = await readErrorText(res);
      throw new Error(msg);
    }

    // { slots: [{inicio, fim}], motivo: "FERIADO"/"FIM_DE_SEMANA"/"OCUPADO", feriadoNome: "Tiradentes" }
    const dataJson = await res.json();

    const slots = Array.isArray(dataJson.slots) ? dataJson.slots : [];
    const motivo = dataJson.motivo;
    const feriadoNome = dataJson.feriadoNome;

    if (slots.length === 0) {
      slotsVazioEl.hidden = false;

      if (motivo === "FERIADO") {
        showAlert(statusEl, "info", `Este dia é feriado: ${feriadoNome || "Feriado"}.`);
      } else if (motivo === "FIM_DE_SEMANA") {
        showAlert(statusEl, "info", "Fim de semana: não há horários disponíveis. Escolha uma data de segunda a sexta.");
      } else if (motivo === "OCUPADO") {
        showAlert(statusEl, "info", "Nenhum horário disponível: todos os horários do dia já estão ocupados.");
      } else {
        showAlert(statusEl, "info", "Nenhum horário disponível para esta data.");
      }
      return;
    }

    slotsVazioEl.hidden = true;
    showAlert(statusEl, "ok", `Encontrados ${slots.length} horários disponíveis. Selecione um para agendar.`);

    for (const slot of slots) {
      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = "slot";

      const inicioTxt = formatarDataHora(slot.inicio);
      const fim = new Date(slot.fim);
      const fimTxt = `${pad2(fim.getHours())}:${pad2(fim.getMinutes())}`;

      btn.innerHTML = `
        <div><strong>${inicioTxt}</strong></div>
        <small>Termina às ${fimTxt}</small>
      `;

      btn.addEventListener("click", () => {
        document.querySelectorAll(".slot.selected").forEach(x => x.classList.remove("selected"));
        btn.classList.add("selected");
        slotSelecionado = slot;
        elSelecionado.value = inicioTxt;
        btnAgendar.disabled = false;
        // ao selecionar novo horário, limpamos confirmação anterior
        showAlert(confirmacaoEl, "", "");
      });

      horariosEl.appendChild(btn);
    }
  } catch (e) {
    showAlert(statusEl, "err", `Erro ao buscar horários: ${e.message}`);
  } finally {
    btnBuscar.disabled = false;
  }
}

async function criarAgendamento() {
  showAlert(confirmacaoEl, "", "");

  if (!slotSelecionado) {
    showAlert(confirmacaoEl, "info", "Selecione um horário antes de agendar.");
    return;
  }

  const nome = (elNome.value || "").trim();
  if (!nome) {
    showAlert(confirmacaoEl, "err", "Informe o nome do paciente.");
    return;
  }

  btnAgendar.disabled = true;
  showAlert(confirmacaoEl, "info", "Criando agendamento...");

  const payload = {
    nomePaciente: nome,
    emailPaciente: (elEmail.value || "").trim() || null,
    inicio: slotSelecionado.inicio,
    fim: slotSelecionado.fim
  };

  try {
    const res = await fetch("/appointments", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const msg = await readErrorText(res);
      throw new Error(msg);
    }

    const criado = await res.json();

    // Atualiza UI primeiro
    await listarAgendamentos();
    await buscarHorarios();

    // Mostra confirmação por último (assim não será apagada)
    showAlert(confirmacaoEl, "ok", `Agendamento confirmado! ID: ${criado.id} • ${formatarDataHora(criado.inicio)}`);
  } catch (e) {
    showAlert(confirmacaoEl, "err", `Erro ao agendar: ${e.message}`);
  } finally {
    btnAgendar.disabled = false;
  }
}

async function listarAgendamentos() {
  listaEl.innerHTML = "";
  listaVaziaEl.hidden = true;

  try {
    const res = await fetch("/appointments");
    if (!res.ok) {
      listaVaziaEl.hidden = false;
      return;
    }

    const itens = await res.json();
    if (!Array.isArray(itens) || itens.length === 0) {
      listaVaziaEl.hidden = false;
      return;
    }

    for (const a of itens) {
      const li = document.createElement("li");
      li.textContent = `${a.nomePaciente} • ${formatarDataHora(a.inicio)}`;
      listaEl.appendChild(li);
    }
  } catch {
    listaVaziaEl.hidden = false;
  }
}

btnBuscar.addEventListener("click", buscarHorarios);
btnAgendar.addEventListener("click", criarAgendamento);
btnAtualizar.addEventListener("click", listarAgendamentos);

elData.addEventListener("change", () => {
  showAlert(statusEl, "", "");
  showAlert(confirmacaoEl, "", "");
  diaInfoEl.textContent = "";
  limparHorarios();
});

listarAgendamentos();