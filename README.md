# Local ICMP Host Test - LIHT

O programa é uma ferramenta de simulação de rede projetada para desenvolvedores e administradores de sistemas. 
Ele permite simular um laboratório de rede com alguns endereços IPs e comportamentos de porta configuráveis em uma máquina Windows, utilizando um Switch Interno dedicado do Hyper-V para isolamento total.

<img width="943" height="506" alt="image" src="https://github.com/user-attachments/assets/c7a0f1c3-6906-4bcb-af31-828a48fa95ec" />

---

## Principais Funcionalidades

- **Simulação Multi-IP Realista**: Crie hosts de laboratório (ex: `198.51.100.1` a `198.51.100.6`) em um adaptador de rede virtual isolado.
- **Sincronização em Tempo Real**: Ative ou desative hosts na interface para adicionar ou remover IPs instantaneamente — simulando falhas de conectividade (ping down) imediatamente.
- **Emulação de Serviços**: Simule FTP, SSH, HTTP, RDP com banners e respostas customizadas.
- **Cópia Rápida**: Botão **Copy** integrado por linha para capturar o IP de qualquer host ao clipboard.
- **Switch Isolado (LIHT-Net)**: Cria automaticamente um Switch Interno dedicado no Hyper-V. Nenhum tráfego de laboratório interfere na internet real, Docker ou WSL.
- **Cleanup de Rede**: Botão **Cleanup Network** para remover completamente o switch virtual `LIHT-Net` e todos os IPs associados, deixando o sistema limpo.

---

## Modos de Porta

| Modo | Descrição |
| :--- | :--- |
| **Banner** | Envia uma string de texto (ex: banner de versão do serviço) ao conectar via TCP. |
| **HttpStatic** | Serve uma resposta HTTP 200 OK com corpo de texto personalizado. |
| **OpenSilent** | Aceita a conexão TCP mas não envia resposta (modo stealth). |
| **UdpEcho** | Responde via UDP repetindo os dados recebidos ou enviando uma resposta fixa. |

---

## Pré-requisitos

- **S.O.**: Testado no Windows 10 (Hyper-V habilitado).
- **Permissões**: Deve ser executado como **Administrador** — necessário para criar o switch e gerenciar IPs.
- **Framework**: .NET 9.0 Runtime.

---

## Configuração (`config.json`)

```json
{
  "InterfaceAlias": "AUTO",
  "BaseNetwork": "198.51.100.0/24",
  "Hosts": [
    {
      "Name": "Gateway-001",
      "IpAddress": "198.51.100.1",
      "Enabled": true,
      "Ports": [
        { "Port": 3389, "Mode": "Banner", "Response": "RDP Service Ready" }
      ]
    }
  ]
}
```

---

## Como Usar

1. **Abra como Administrador**: Clique com o botão direito em `LIHT.exe` → **Executar como Administrador**.
2. **Inicie o Motor**: Clique em **Start Engine**. O log confirmará a criação/uso do switch `LIHT-Net`.
3. **Teste de Conectividade**:
   - `ping 198.51.100.1` para verificar o Gateway.
   - Botão **Copy** para copiar o IP de qualquer host.
4. **Simule Falhas**: Desmarque **Active** em qualquer host. O IP é removido instantaneamente da placa — o ping para de responder.
5. **Limpeza**: Clique em **Cleanup Network** para remover o switch `LIHT-Net` do Hyper-V completamente.

---

## Observação Técnica

O LIHT usa a faixa `198.51.100.0/24` (RFC 5737), garantidamente livre de conflitos com redes locais, Docker (`172.x.x.x`), WSL e VPNs corporativas.

O switch `LIHT-Net` é do tipo **Internal**, portanto o tráfego nunca sai para sua rede física.

---

## Design e UX

- **Status Visual**: 🟢 Online, 🔴 Offline, ⚫ Desativado — atualizado a cada 3 segundos.
- **Log Dinâmico**: Mensagens com timestamps e erros de PowerShell em tempo real.
- **Fechar = Sair**: 
