import "./App.css";
import NavBar from "./components/NavBar";
import useWledAddresses from "./hooks/useWledAddresses";
import { Text, Grid, GridItem, Show } from "@chakra-ui/react";

function App() {
  //const { wledAddresses, error, isLoading, apiClient } = useWledAddresses();

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
        paddingTop={"100px"}
      >
        <GridItem area="nav">
          <NavBar
            onSearch={function (searchText: string): void {
              throw new Error("Function not implemented.");
            }}
          ></NavBar>
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
