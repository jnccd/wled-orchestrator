import "./App.css";
import NavBar from "./components/NavBar";
import { Text, Grid, GridItem, Divider } from "@chakra-ui/react";
import WledSegmentGroupsViewer from "./components/WledSegmentGroupsViewer/WledSegmentGroupsViewer";
import WledOrchThemeEditor from "./components/WledOrchThemeEditor/WledOrchThemeEditor";
import useSelectedGroupStore from "./hooks/useLocalStore";

function App() {
  const selectedGroupStore = useSelectedGroupStore();
  selectedGroupStore.initialize();

  return (
    <>
      <Grid
        templateAreas={{
          base: `"nav" "main"`,
        }}
        templateColumns={{
          base: "1fr",
        }}
        width={"100%"}
        height={"100%"}
      >
        <GridItem area="nav">
          <NavBar></NavBar>
        </GridItem>
        <GridItem area="main">
          <br />
          <WledSegmentGroupsViewer></WledSegmentGroupsViewer>
          <br />
          <Divider width="90%" margin="0rem auto" opacity={1} />
          <br />
          <WledOrchThemeEditor></WledOrchThemeEditor>
          {[...Array(50).keys()].map((i) => (
            <Text key={i}>Heeey</Text>
          ))}
        </GridItem>
      </Grid>
    </>
  );
}

export default App;
