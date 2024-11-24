import "./App.css";
import NavBar from "./components/NavBar";
import { Text, Grid, GridItem, Show } from "@chakra-ui/react";

function App() {
  return (
    <>
      <Grid
        templateAreas={{
          base: `"nav" "main"`,
          lg: `"nav nav" 
             "aside main"`,
        }}
        templateColumns={{
          base: "1fr",
          lg: "200px 1fr",
        }}
        width={"100%"}
        height={"100%"}
      >
        <GridItem area="nav">
          <NavBar></NavBar>
        </GridItem>
        <Show above="lg">
          <GridItem area="aside" paddingX={5}>
            Sidebar?
          </GridItem>
        </Show>
        <GridItem area="main">
          {[...Array(50).keys()].map((_) => (
            <Text>Heeey</Text>
          ))}
        </GridItem>
      </Grid>
    </>
  );
}

export default App;
