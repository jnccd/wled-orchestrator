import { useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.css";
import ListGroup from "./components/ListGroup";
import useWledAddresses from "./hooks/useWledAddresses";

function App() {
  const [count, setCount] = useState(0);
  const { wledAddresses, error, isLoading, apiClient } = useWledAddresses();

  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Wled Orchestrator</h1>

      {
        <p>
          {error}
          {apiClient.defaults.baseURL}
        </p>
      }
      {isLoading && (
        <ListGroup items={["Loading.", "Loading..", "Loading..."]}></ListGroup>
      )}
      {!isLoading && <ListGroup items={wledAddresses.addresses}></ListGroup>}

      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  );
}

export default App;
