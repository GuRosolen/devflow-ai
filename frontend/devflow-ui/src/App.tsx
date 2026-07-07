import { useEffect, useState } from 'react';
import axios from 'axios';

interface TaskItem {
  id: number;
  title: string;
  description: string;
  columnId: number;
}

function App() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [novoTitulo, setNovoTitulo] = useState('');
  const [novaDescricao, setNovaDescricao] = useState('');
  const [gerandoIA, setGerandoIA] = useState(false); // Estado para o botão da IA

  const apiUrl = 'https://devflow-ai-api-backend.onrender.com';

  const buscarTarefas = async () => {
    try {
      const res1 = await axios.get(`${apiUrl}/columns/1/tasks`);
      const res2 = await axios.get(`${apiUrl}/columns/2/tasks`);
      const res3 = await axios.get(`${apiUrl}/columns/3/tasks`);
      setTasks([...res1.data, ...res2.data, ...res3.data]);
    } catch (error) {
      console.error("Erro ao buscar dados:", error);
    }
  };

  useEffect(() => {
    buscarTarefas();
  }, []);

  const criarTarefa = (e: React.FormEvent) => {
    e.preventDefault();
    if (!novoTitulo) return;

    const novaTarefa = {
      title: novoTitulo,
      description: novaDescricao,
      columnId: 1
    };

    axios.post(`${apiUrl}/tasks`, novaTarefa)
      .then(() => {
        setNovoTitulo('');
        setNovaDescricao('');
        buscarTarefas();
      })
      .catch(error => console.error("Erro ao criar:", error));
  };

  const moverTarefa = (taskId: number, novaColunaId: number) => {
    axios.put(`http://localhost:5009/tasks/${taskId}/move?newColumnId=${novaColunaId}`)
      .then(() => buscarTarefas())
      .catch(error => console.error("Erro ao mover:", error));
  };

  // Função que chama a nossa rota de Inteligência Artificial
  const pedirDicasIA = async (e: React.MouseEvent) => {
    e.preventDefault(); // Evita que o formulário recarregue a página
    if (!novoTitulo) {
      alert("Digite um título primeiro para a IA saber do que se trata!");
      return;
    }

    setGerandoIA(true);
    try {
      // Mandamos um objeto com o título para a rota da IA
      const response = await axios.post('http://localhost:5009/ai/suggest', {
        title: novoTitulo,
        description: "",
        columnId: 1
      });
      // Preenche o campo de descrição com a resposta do Gemini
      setNovaDescricao(response.data.suggestion);
    } catch (error) {
      console.error("Erro na IA:", error);
      alert("Erro ao gerar sugestão. O backend está rodando e a chave está correta?");
    } finally {
      setGerandoIA(false);
    }
  };

  const renderizarColuna = (titulo: string, colunaId: number, corBorda: string) => {
    const tarefasDaColuna = tasks.filter(task => task.columnId === colunaId);

    return (
      <div style={{ backgroundColor: '#f8f9fa', padding: '20px', borderRadius: '12px', width: '320px', boxShadow: '0 4px 6px rgba(0,0,0,0.05)', display: 'flex', flexDirection: 'column' }}>
        <h2 style={{ fontSize: '18px', color: '#495057', borderBottom: '2px solid #dee2e6', paddingBottom: '10px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          {titulo}
          <span style={{ backgroundColor: '#e9ecef', padding: '4px 10px', borderRadius: '20px', fontSize: '12px', color: '#6c757d' }}>
            {tarefasDaColuna.length}
          </span>
        </h2>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '15px', marginTop: '15px', flexGrow: 1 }}>
          {tarefasDaColuna.map(task => (
            <div key={task.id} style={{ backgroundColor: '#ffffff', padding: '15px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)', borderLeft: `4px solid ${corBorda}` }}>
              <strong style={{ display: 'block', fontSize: '16px', color: '#212529', marginBottom: '8px' }}>
                {task.title}
              </strong>
              {task.description && (
                <p style={{ margin: '0 0 15px 0', fontSize: '14px', color: '#6c757d', lineHeight: '1.4', whiteSpace: 'pre-wrap' }}>
                  {task.description}
                </p>
              )}

              <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid #f1f3f5', paddingTop: '10px' }}>
                <button
                  onClick={() => moverTarefa(task.id, colunaId - 1)}
                  disabled={colunaId === 1}
                  style={{ background: 'none', border: 'none', cursor: colunaId === 1 ? 'not-allowed' : 'pointer', opacity: colunaId === 1 ? 0 : 1, color: '#495057', fontWeight: 'bold' }}
                >
                  ⬅️ Voltar
                </button>
                <button
                  onClick={() => moverTarefa(task.id, colunaId + 1)}
                  disabled={colunaId === 3}
                  style={{ background: 'none', border: 'none', cursor: colunaId === 3 ? 'not-allowed' : 'pointer', opacity: colunaId === 3 ? 0 : 1, color: '#495057', fontWeight: 'bold' }}
                >
                  Avançar ➡️
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <div style={{ padding: '40px', fontFamily: 'system-ui, Arial, sans-serif', backgroundColor: '#e9ecef', minHeight: '100vh' }}>
      <h1 style={{ color: '#343a40', marginBottom: '30px' }}>DevFlow AI 🚀</h1>

      <div style={{ backgroundColor: '#fff', padding: '20px', borderRadius: '8px', width: '320px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)', marginBottom: '30px' }}>
        <h3 style={{ margin: '0 0 15px 0', color: '#495057' }}>Nova Tarefa</h3>
        <form onSubmit={criarTarefa} style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
          <input
            type="text"
            placeholder="Título da tarefa..."
            value={novoTitulo}
            onChange={(e) => setNovoTitulo(e.target.value)}
            style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ced4da' }}
          />

          {/* Novo Botão da IA */}
          <button
            type="button"
            onClick={pedirDicasIA}
            disabled={gerandoIA}
            style={{ padding: '8px', backgroundColor: '#6f42c1', color: '#fff', border: 'none', borderRadius: '4px', cursor: gerandoIA ? 'wait' : 'pointer', fontSize: '13px', fontWeight: 'bold', display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '5px' }}
          >
            {gerandoIA ? '⏳ Pensando...' : '✨ Sugerir subtarefas com IA'}
          </button>

          <textarea
            placeholder="Descrição (opcional)..."
            value={novaDescricao}
            onChange={(e) => setNovaDescricao(e.target.value)}
            style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ced4da', resize: 'vertical', minHeight: '80px' }}
          />
          <button type="submit" style={{ padding: '10px', backgroundColor: '#28a745', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
            + Adicionar ao To Do
          </button>
        </form>
      </div>

      <div style={{ display: 'flex', gap: '20px', overflowX: 'auto', paddingBottom: '10px' }}>
        {renderizarColuna("To Do 📌", 1, "#007bff")}
        {renderizarColuna("Doing ⏳", 2, "#ffc107")}
        {renderizarColuna("Done ✅", 3, "#28a745")}
      </div>
    </div>
  );
}

export default App;