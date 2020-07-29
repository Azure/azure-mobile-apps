/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 *
 * Generated with the TypeScript template
 * https://github.com/react-native-community/react-native-template-typescript
 *
 * @format
 */
import "react-native-gesture-handler";
import * as React from "react";
import {
    SafeAreaView,
    StyleSheet,
    ScrollView,
    View,
    Text,
    StatusBar,
    Image,
    Dimensions,
    Button,
    Alert
} from "react-native";

import { NavigationContainer } from "@react-navigation/native";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import Icons from "react-native-vector-icons/FontAwesome";
import CustomList from "./listComponents/CustomList";
import { TokenCredential, GetTokenOptions, AccessToken } from '@azure/identity';
import { MobileDataClient, MobileDataTable } from "@azure/mobile-client";

const { width } = Dimensions.get("window");

function getTestClient() {
    const tokenCredential: TokenCredential = {
        getToken(scope: string | string[], _?: GetTokenOptions): Promise<AccessToken | null> {
            return Promise.resolve({
                token: "some-test-access-credential: " + scope,
                expiresOnTimestamp: 3600
            });
        }
    };
    const client = new MobileDataClient("foo://url.com", tokenCredential, undefined);
    const dataTable = new MobileDataTable(client, "test-table");

    return dataTable;
}

function getData() {
    return [
        {
            key: 1,
            title: "Sachin Tendulkar",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "https://th.bing.com/th/id/OIP.8bJLT7pglSdcG1VLnnwTzgHaJh?pid=Api&rs=1"
        },
        {
            key: 2,
            title: "Isaac newton",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "http://4.bp.blogspot.com/-WzJ6SRiUceY/Tl_sSyI33xI/AAAAAAAAAvk/T9qZV5kDedU/s1600/108439.jpg"
        },
        {
            key: 3,
            title: "Albert Einstein",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "https://th.bing.com/th/id/OIP.8bJLT7pglSdcG1VLnnwTzgHaJh?pid=Api&rs=1"
        },
        {
            key: 4,
            title: "Isaac newton",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "http://4.bp.blogspot.com/-WzJ6SRiUceY/Tl_sSyI33xI/AAAAAAAAAvk/T9qZV5kDedU/s1600/108439.jpg"
        },
        {
            key: 5,
            title: "Albert Einstein",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "https://th.bing.com/th/id/OIP.8bJLT7pglSdcG1VLnnwTzgHaJh?pid=Api&rs=1"
        },
        {
            key: 6,
            title: "Isaac newton",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "http://4.bp.blogspot.com/-WzJ6SRiUceY/Tl_sSyI33xI/AAAAAAAAAvk/T9qZV5kDedU/s1600/108439.jpg"
        },
        {
            key: 7,
            title: "Albert Einstein",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "https://th.bing.com/th/id/OIP.8bJLT7pglSdcG1VLnnwTzgHaJh?pid=Api&rs=1"
        },
        {
            key: 8,
            title: "Isaac newton",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "http://4.bp.blogspot.com/-WzJ6SRiUceY/Tl_sSyI33xI/AAAAAAAAAvk/T9qZV5kDedU/s1600/108439.jpg"
        },
        {
            key: 9,
            title: "Albert Einstein",
            time: "2 months ago",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",
            image_url: "https://th.bing.com/th/id/OIP.8bJLT7pglSdcG1VLnnwTzgHaJh?pid=Api&rs=1"
        }

    ]
}

function HomeScreen() {

    const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    const monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    var d = new Date();
    var dayName = days[d.getDay()];
    var date = d.getDate(); //Current Date
    var month = monthNames[d.getMonth()]; //Current Month

    return (
        <View style={styles.MainContainer}>
            <StatusBar barStyle="dark-content" />
            <SafeAreaView>
                <View style={styles.body}>
                    <View
                        style={{
                            flexDirection: "row",
                            height: 100
                        }}>
                        <View style={styles.container}>
                            <View style={{ flex: 0.5 }} >
                                <Text style={styles.dateTitle}>
                                    {dayName} {date} {month}
                                </Text>
                            </View>
                            <View style={{ flex: 0.5 }} >
                                <Text style={styles.mainTitle}>
                                    Blog
                </Text>
                            </View>
                        </View>

                        <View style={{ flex: 0.4 }} />
                        <View style={{ flex: 0.3 }} >
                            <Image source={{ uri: "https://reactjs.org/logo-og.png" }}
                                style={{ width: 50, height: 50, justifyContent: "center", marginTop: 15 }} />
                        </View>

                    </View>

                    <ScrollView
                        horizontal={true}
                        decelerationRate={0}
                        snapToInterval={width - 60}
                        snapToAlignment={"center"}
                        contentInset={{
                            top: 0,
                            left: 30,
                            bottom: 0,
                            right: 30,
                        }}>
                        <View style={styles.view} />
                        <View style={styles.view2} />
                        <View style={styles.view} />
                        <View style={styles.view2} />
                    </ScrollView>


                    <View
                        style={{
                            flexDirection: "row",
                            height: 100
                        }}>
                        <Text style={styles.sectionTitle}>
                            Popular
              </Text>
                        <View style={{ flex: 1 }} />
                        <View style={styles.buttonMargin}>
                            <Button
                                color="#ffa500"
                                title="Show all"
                                onPress={() => Alert.alert("Simple Button pressed")} />
                        </View>
                    </View>
                    <View style={styles.listMargin}>
                        <CustomList
                            itemList={getData()}
                        />
                    </View>
                </View>
            </SafeAreaView>
        </View>
    );
}

function EditPost() {

    const [authResult, setAuthResult] = React.useState<MSALResult | null>(null);
    const [prefersEphemeralWebBrowserSession, setPrefersEphemeralWebBrowserSession] = React.useState<boolean>(false);
    const handleResult = (result: MSALResult) => {
        setAuthResult(result);
    };

    var authJson;

    const acquireToken = async () => {
        try {
            const res = await msalClient.acquireToken({
                authority: msalConfig.sisuAuthority,
                scopes: msalConfig.scopes,
                ios_prefersEphemeralWebBrowserSession: true,
            });
            handleResult(res);
        } catch (error) {
            console.warn(error);
        }
    };

    const acquireTokenSilent = async () => {
        if (authResult) {
            try {
                const res = await msalClient.acquireTokenSilent({
                    authority: msalConfig.sisuAuthority,
                    scopes: msalConfig.scopes,
                    accountIdentifier: authResult.account.identifier,
                });
                handleResult(res);
            } catch (error) {
                console.warn(error);
            }
        }
    };

    const removeAccount = async () => {
        if (authResult) {
            try {
                await msalClient.removeAccount({
                    authority: msalConfig.sisuAuthority,
                    accountIdentifier: authResult.account.identifier,
                });
                setAuthResult(null);
            } catch (error) {
                console.warn(error);
            }
        }
    };

    const signout = async () => {
        if (authResult) {
            try {
                await msalClient.signout({
                    authority: msalConfig.sisuAuthority,
                    accountIdentifier: authResult.account.identifier,
                    ios_prefersEphemeralWebBrowserSession: prefersEphemeralWebBrowserSession,
                });
                setAuthResult(null);
            } catch (error) {
                console.warn(error);
            }
        }
    };


    function parseToken() {
        var temp = JSON.stringify(authResult, null, 4);
        var test = JSON.parse(temp, (key, value) => {
            if (key === "accessToken") {
                return value.toString();
            }
            return null;
        });

        authJson = test;
    }

    // const postBlog = async () => {
    //   if (authResult) {
    //     try {
    //         await fetch('https://blogserver-zumo-next.azurewebsites.net/tables/blogcomments', {
    //           method: 'POST',
    //           headers: new Headers({
    //             'Authorization': 'bearer '+{authResult}},
    //             'Content-Type': 'application/json'
    //           }),
    //           body: JSON.stringify({
    //             "text" : "Popcorn",
    //             "postId": "a3c54f73bbca4a51a08b6908d6176feb"
    //           })
    //         });
    //     } catch (error) {
    //         console.warn(error);
    //     }
    //   }
    // };

    return (
        <View style={styles.MainContainer}>
            <StatusBar barStyle="dark-content" />
            <SafeAreaView>
                <View style={styles.otherPage}>
                    <Text style={styles.otherMainTitle}>
                        Edit Post
                </Text>

                    <Button title="Acquire Token" onPress={acquireToken} />
                    <Button title="Acquire Token Silently" onPress={acquireTokenSilent} disabled={!authResult} />
                    <Button title="Remove account" onPress={removeAccount} disabled={!authResult} />
                    <Button title="Parse Auth Token" onPress={parseToken} />
                    {Platform.OS === 'ios' && <Button title="Sign out (iOS only)" onPress={signout} disabled={!authResult} />}
                    {Platform.OS === 'ios' && (
                        <View style={styles.switch}>
                            <View style={styles.switchSpacer} />
                            <View style={styles.switchLabel}>
                                <Text
                                    onPress={() => setPrefersEphemeralWebBrowserSession(!prefersEphemeralWebBrowserSession)}
                                    style={styles.text}
                                >
                                    Prefer ephemeral web browser session?
              {'\n'}
              (iOS only)
            </Text>
                            </View>
                            <View style={styles.switchSpacer}>
                                <Switch value={prefersEphemeralWebBrowserSession} onValueChange={setPrefersEphemeralWebBrowserSession} />
                            </View>
                        </View>
                    )}
                    <ScrollView >
                        <Text>{JSON.stringify(authResult, null, 4)}</Text>
                        <Text>
                            AUTH TOKEN
                            ********************************
                            *********************************
               </Text>

                        <Text>{authJson}</Text>
                        <Text>
                            *********************************
                            ****************************************
                            AUTH TOKEN
               </Text>
                    </ScrollView>
                </View>
            </SafeAreaView>
        </View>
    );
}

function Bookmarks() {
    return (
        <View style={styles.MainContainer}>
            <StatusBar barStyle="dark-content" />
            <SafeAreaView>
                <View style={styles.otherPage}>
                    <Text style={styles.otherMainTitle}>
                        Bookmarks
                </Text>

                    <View style={styles.bookmarkMargin}>
                        <CustomList
                            itemList={getData()}
                        />
                    </View>
                </View>
            </SafeAreaView>
        </View>
    );
}

function Profile() {
    return (
        <View style={styles.MainContainer}>
            <StatusBar barStyle="dark-content" />
            <SafeAreaView>
                <View style={styles.otherPage}>
                    <Text style={styles.otherMainTitle}>
                        Profile
                </Text>

                    <View style={styles.bookmarkMargin}>
                        <Text>
                            Profile Here
            </Text>
                    </View>
                </View>
            </SafeAreaView>
        </View>
    );
}

const Tab = createBottomTabNavigator();

const App = () => {
    return (

        <NavigationContainer>
            <Tab.Navigator
                screenOptions={({ route }) => ({
                    tabBarIcon: ({ focused, color, size }) => {
                        let iconName;

                        if (route.name === 'Home') {
                            iconName = focused
                                ? 'home'
                                : 'home';
                        } else if (route.name === 'Edit') {
                            iconName = focused ? 'pencil-square' : 'pencil-square';
                        } else if (route.name === 'Bookmark') {
                            iconName = focused ? 'bookmark' : 'bookmark';
                        } else if (route.name === 'Profile') {
                            iconName = focused ? 'user-circle' : 'user-circle';
                        }

                        // You can return any component that you like here!
                        return <Icons name={iconName} size={size} color={color} />;
                    },
                })}
                tabBarOptions={{
                    activeTintColor: 'black',
                    inactiveTintColor: 'gray',
                }}
            >
                <Tab.Screen name="Home" component={HomeScreen} />
                <Tab.Screen name="Edit" component={EditPost} />
                <Tab.Screen name="Bookmark" component={Bookmarks} />
                <Tab.Screen name="Profile" component={Profile} />
            </Tab.Navigator>
        </NavigationContainer>

    );
};

const styles = StyleSheet.create({
    MainContainer: {
        backgroundColor: '#FFFFFF'
    },
    body: {
        padding: 30
    },
    otherPage: {
        padding: 10
    },
    dateTitle: {
        fontSize: 16,
        fontWeight: '600',
        color: '#ffa500',
        marginTop: 10
    },
    mainTitle: {
        fontSize: 36,
        fontWeight: '600',
        color: '#000000',
        justifyContent: "center",
        marginTop: -10
    },
    otherMainTitle: {
        fontSize: 36,
        fontWeight: '600',
        color: '#000000',
        justifyContent: "center",
        marginTop: 10,
        marginLeft: 10
    },
    sectionTitle: {
        fontSize: 28,
        fontWeight: '600',
        color: '#000000',
        justifyContent: "center",
        marginTop: 10
    },
    buttonMargin: {
        marginTop: 10
    },
    container: {
        flex: 1,
        flexDirection: 'column',
    },
    listMargin: {
        marginTop: -40
    },
    bookmarkMargin: {
        marginTop: 10
    },
    view: {
        marginTop: 30,
        backgroundColor: 'blue',
        width: width - 130,
        margin: 10,
        height: 250,
        borderRadius: 10,
        //paddingHorizontal : 30
    },
    view2: {
        marginTop: 30,
        backgroundColor: 'red',
        width: width - 130,
        margin: 10,
        height: 250,
        borderRadius: 10,
        //paddingHorizontal : 30
    },
    switch: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    text: {
        textAlign: 'center',
    },
    switchSpacer: {
        flex: 1,
    },
    switchLabel: {
        flexGrow: 0,
        padding: 10,
    },
});

export default App;
