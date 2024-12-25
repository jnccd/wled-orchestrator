import "./App.css";
import NavBar from "./components/NavBar";
import { Text, Grid, GridItem } from "@chakra-ui/react";
import WledSegmentGroupViewer from "./components/WledSegmentGroupViewer";

function App() {
  const serverButtonIdPrefix = "server-button";

  return (
    <>
      <Grid
        templateAreas={{
          base: `"nav" "main"`,
        }}
        templateColumns={{
          base: "1fr",
        }}
        width={"100vw"}
        height={"100%"}
      >
        <GridItem area="nav">
          <NavBar></NavBar>
        </GridItem>
        <GridItem area="main">
          <WledSegmentGroupViewer
            serverButtonIdPrefix={serverButtonIdPrefix}
          ></WledSegmentGroupViewer>
          {[...Array(50).keys()].map((_) => (
            <Text>Heeey</Text>
          ))}
        </GridItem>
      </Grid>
    </>
  );
}

export default App;
